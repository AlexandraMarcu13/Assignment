import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { AuthService } from '../../services/auth.service';
import { OrderRequest, CartItem } from '../../models/cart.model';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent implements OnInit {
  orderData: OrderRequest = {
    shippingAddress: '',
    city: '',
    postalCode: '',
    country: '',
    items: []
  };
  cartItems: CartItem[] = [];
  isProcessing = false;
  submitted = false;

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cartItems = this.cartService.getCartItems();
    this.orderData.items = this.cartItems;
    
    if (this.cartItems.length === 0) {
      this.router.navigate(['/cart']);
    }
  }

  getTotal(): number {
    return this.cartService.getTotal();
  }

  isFormValid(): boolean {
    return !!(this.orderData.shippingAddress?.trim() && 
              this.orderData.city?.trim() && 
              this.orderData.postalCode?.trim() && 
              this.orderData.country?.trim());
  }

  placeOrder(): void {
    this.submitted = true;
    
    if (!this.authService.isAuthenticated()) {
      alert('Please login to place order');
      this.router.navigate(['/login']);
      return;
    }

    if (!this.isFormValid()) {
      alert('Please fill in all shipping information');
      return;
    }

    this.isProcessing = true;
    
    this.orderService.createOrder(this.orderData).subscribe({
      next: (order) => {
        this.cartService.clearCart();
        alert('Order placed successfully!');
        this.router.navigate(['/products']);
      },
      error: (error) => {
        console.error('Error placing order:', error);
        
        let errorMessage = 'Error placing order. ';
        if (error.error?.message) {
          errorMessage += error.error.message;
        } else if (error.status === 400) {
          errorMessage += 'Invalid order data. Please check your cart items.';
        } else if (error.status === 401) {
          errorMessage += 'Please login again.';
          this.authService.logout();
          this.router.navigate(['/login']);
        } else {
          errorMessage += 'Please try again.';
        }
        
        alert(errorMessage);
        this.isProcessing = false;
      }
    });
  }
}