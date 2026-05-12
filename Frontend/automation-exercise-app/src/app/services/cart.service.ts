import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { CartItem } from '../models/cart.model';
import { Product } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartItemsSubject = new BehaviorSubject<CartItem[]>([]);
  public cartItems$ = this.cartItemsSubject.asObservable();
  private cartCountSubject = new BehaviorSubject<number>(0);
  public cartCount$ = this.cartCountSubject.asObservable();
  private productStockMap = new Map<number, number>(); 

  constructor() {
    this.loadCart();
  }

  private loadCart(): void {
    const savedCart = localStorage.getItem('cart');
    if (savedCart) {
      const items = JSON.parse(savedCart);
      this.cartItemsSubject.next(items);
      this.updateCartCount();
    }
  }

  private saveCart(items: CartItem[]): void {
    localStorage.setItem('cart', JSON.stringify(items));
    this.cartItemsSubject.next(items);
    this.updateCartCount();
  }

  private updateCartCount(): void {
    const items = this.cartItemsSubject.value;
    const count = items.reduce((sum, item) => sum + item.quantity, 0);
    this.cartCountSubject.next(count);
  }

  updateProductStock(productId: number, stock: number): void {
    this.productStockMap.set(productId, stock);
  }

  getQuantity(productId: number): number {
    const items = this.cartItemsSubject.value;
    const item = items.find(i => i.productId === productId);
    return item ? item.quantity : 0;
  }

  getAvailableStock(productId: number): number {
    const totalStock = this.productStockMap.get(productId) || 0;
    const currentQuantity = this.getQuantity(productId);
    return totalStock - currentQuantity;
  }

  addToCart(product: Product): { success: boolean; message: string } {
    const currentQuantity = this.getQuantity(product.id);
    const availableStock = product.stock - currentQuantity;
    
    if (availableStock <= 0) {
      return { success: false, message: `Only ${product.stock} items available in stock` };
    }
    
    const currentItems = this.cartItemsSubject.value;
    const existingItem = currentItems.find(item => item.productId === product.id);

    if (existingItem) {
      existingItem.quantity++;
      existingItem.totalPrice = existingItem.price * existingItem.quantity;
      this.saveCart(currentItems);
      return { success: true, message: `${product.name} quantity increased to ${existingItem.quantity}` };
    } else {
      const newItem: CartItem = {
        productId: product.id,
        productName: product.name,
        price: product.price,
        quantity: 1,
        totalPrice: product.price
      };
      this.saveCart([...currentItems, newItem]);
      return { success: true, message: `${product.name} added to cart!` };
    }
  }

  updateQuantity(productId: number, quantity: number, maxStock: number): { success: boolean; message: string } {
    const currentItems = this.cartItemsSubject.value;
    const item = currentItems.find(i => i.productId === productId);
    
    if (item) {
      if (quantity > maxStock) {
        return { success: false, message: `Only ${maxStock} items available in stock` };
      }
      
      if (quantity <= 0) {
        this.removeItem(productId);
        return { success: true, message: `Item removed from cart` };
      } else {
        item.quantity = quantity;
        item.totalPrice = item.price * quantity;
        this.saveCart(currentItems);
        return { success: true, message: `Quantity updated to ${quantity}` };
      }
    }
    return { success: false, message: `Item not found in cart` };
  }

  removeItem(productId: number): void {
    const currentItems = this.cartItemsSubject.value;
    const updatedItems = currentItems.filter(item => item.productId !== productId);
    this.saveCart(updatedItems);
  }

  clearCart(): void {
    this.saveCart([]);
  }

  getTotal(): number {
    return this.cartItemsSubject.value.reduce((sum, item) => sum + item.totalPrice, 0);
  }

  getCartItems(): CartItem[] {
    return this.cartItemsSubject.value;
  }
}