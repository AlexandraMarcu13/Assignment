import { TestBed } from '@angular/core/testing';
import { CartService } from './cart.service';
import { Product } from '../models/product.model';

describe('CartService', () => {
  let service: CartService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CartService);
    localStorage.clear();
    service.clearCart();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('addToCart', () => {
    const mockProduct: Product = {
      id: 1,
      name: 'Test Product',
      price: 100,
      stock: 3,
      description: 'Test',
      category: 'Test',
      imageUrl: 'test.jpg'
    };

    it('should not exceed stock limit', () => {
      service.updateProductStock(1, 3);
      service.addToCart(mockProduct);
      service.addToCart(mockProduct);
      service.addToCart(mockProduct);
      
      const items = service.getCartItems();
      expect(items[0].quantity).toBe(3);
      
      const result = service.addToCart(mockProduct);
      expect(result.success).toBeFalse();
      expect(result.message).toContain('Only 3 items available');
    });
  });
});