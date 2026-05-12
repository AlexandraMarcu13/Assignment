import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { ProductService } from '../../services/product.service';
import { CartItem } from '../../models/cart.model';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit {
  cartItems: CartItem[] = [];
  productStockMap = new Map<number, number>();

  constructor(
    private cartService: CartService,
    private productService: ProductService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cartService.cartItems$.subscribe(items => {
      this.cartItems = items;
    });
    this.loadProductStocks();
  }

  loadProductStocks(): void {
    const productIds = [...new Set(this.cartItems.map(item => item.productId))];
    productIds.forEach(productId => {
      this.productService.getProductById(productId).subscribe({
        next: (product) => {
          this.productStockMap.set(productId, product.stock);
          this.cartService.updateProductStock(productId, product.stock);
        },
        error: (error) => {
          console.error(`Failed to load stock for product ${productId}`, error);
        }
      });
    });
  }

  getMaxStock(productId: number): number {
    return this.productStockMap.get(productId) || 999;
  }

  updateQuantity(productId: number, quantity: number): void {
    const maxStock = this.getMaxStock(productId);
    const result = this.cartService.updateQuantity(productId, quantity, maxStock);
    
    if (!result.success) {
      alert(result.message);
      this.cartService.cartItems$.subscribe(items => {
        this.cartItems = items;
      });
    } else if (result.success && result.message !== 'Item removed from cart') {
      console.log(result.message);
    }
  }

  removeItem(productId: number): void {
    this.cartService.removeItem(productId);
  }

  getTotal(): number {
    return this.cartService.getTotal();
  }

  checkout(): void {
    if (this.cartItems.length > 0) {
      this.router.navigate(['/checkout']);
    }
  }
}