import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CartComponent } from './cart.component';
import { CartService } from '../../services/cart.service';
import { ProductService } from '../../services/product.service';
import { CartItem } from '../../models/cart.model';
import { Product } from '../../models/product.model';

describe('CartComponent', () => {
  let component: CartComponent;
  let fixture: ComponentFixture<CartComponent>;
  let mockCartService: jasmine.SpyObj<CartService>;
  let mockProductService: jasmine.SpyObj<ProductService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockCartItems: CartItem[] = [
    { productId: 1, productName: 'Product 1', price: 100, quantity: 2, totalPrice: 200 },
    { productId: 2, productName: 'Product 2', price: 50, quantity: 1, totalPrice: 50 }
  ];

  const mockProduct: Product = {
    id: 1, name: 'Product 1', price: 100, stock: 10,
    description: '', category: '', imageUrl: ''
  };

  beforeEach(async () => {
    mockCartService = jasmine.createSpyObj('CartService', [
      'cartItems$', 'updateQuantity', 'removeItem', 'getTotal', 'getCartItems', 'updateProductStock'
    ]);
    mockProductService = jasmine.createSpyObj('ProductService', ['getProductById']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    mockCartService.cartItems$ = of(mockCartItems);
    mockCartService.getTotal.and.returnValue(250);
    mockCartService.getCartItems.and.returnValue(mockCartItems);
    
    // Mock getProductById to return a product
    mockProductService.getProductById.and.returnValue(of(mockProduct));

    await TestBed.configureTestingModule({
      imports: [CartComponent],
      providers: [
        provideRouter([]),
        { provide: CartService, useValue: mockCartService },
        { provide: ProductService, useValue: mockProductService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('updateQuantity', () => {
    it('should call cartService.updateQuantity', () => {
      mockCartService.updateQuantity.and.returnValue({ success: true, message: 'Updated' });
      
      component.updateQuantity(1, 5);
      
      expect(mockCartService.updateQuantity).toHaveBeenCalled();
    });

    it('should show alert if update fails', () => {
      spyOn(window, 'alert');
      mockCartService.updateQuantity.and.returnValue({ success: false, message: 'Error!' });
      
      component.updateQuantity(1, 10);
      
      expect(window.alert).toHaveBeenCalledWith('Error!');
    });
  });

  describe('removeItem', () => {
    it('should call cartService.removeItem', () => {
      component.removeItem(1);
      expect(mockCartService.removeItem).toHaveBeenCalledWith(1);
    });
  });

  describe('getTotal', () => {
    it('should return total from cartService', () => {
      expect(component.getTotal()).toBe(250);
      expect(mockCartService.getTotal).toHaveBeenCalled();
    });
  });

  describe('checkout', () => {
    it('should navigate to checkout when cart has items', () => {
      component.cartItems = mockCartItems;
      component.checkout();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/checkout']);
    });

    it('should not navigate when cart is empty', () => {
      component.cartItems = [];
      component.checkout();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });
});