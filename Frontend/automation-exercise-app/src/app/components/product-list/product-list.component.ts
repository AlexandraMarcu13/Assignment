import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { Product } from '../../models/product.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  loading = false;

  constructor(
    private cartService: CartService,  // Keep private
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.productService.getAllProducts().subscribe({
      next: (data) => {
        this.products = data;
        this.products.forEach(product => {
          this.cartService.updateProductStock(product.id, product.stock);
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading products:', error);
        alert('Failed to load products. Make sure backend is running on https://localhost:7243');
        this.loading = false;
      }
    });
  }

  addToCart(product: Product): void {
    if (product.stock === 0) {
      alert('Product is out of stock!');
      return;
    }
    
    const result = this.cartService.addToCart(product);
    alert(result.message);
  }
  
  getRemainingStock(product: Product): number {
    return this.cartService.getAvailableStock(product.id);
  }

  getQuantityInCart(productId: number): number {
    return this.cartService.getQuantity(productId);
  }
}