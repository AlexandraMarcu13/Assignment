import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CheckoutComponent } from './checkout.component';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { AuthService } from '../../services/auth.service';
import { FormsModule } from '@angular/forms';
import { CartItem } from '../../models/cart.model';

describe('CheckoutComponent', () => {
  let component: CheckoutComponent;
  let fixture: ComponentFixture<CheckoutComponent>;
  let mockCartService: jasmine.SpyObj<CartService>;
  let mockOrderService: jasmine.SpyObj<OrderService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockCartItems: CartItem[] = [
    { productId: 1, productName: 'Product 1', price: 100, quantity: 2, totalPrice: 200 }
  ];

  beforeEach(async () => {
    mockCartService = jasmine.createSpyObj('CartService', ['getCartItems', 'getTotal', 'clearCart']);
    mockOrderService = jasmine.createSpyObj('OrderService', ['createOrder']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'logout']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    mockCartService.getCartItems.and.returnValue(mockCartItems);
    mockCartService.getTotal.and.returnValue(200);

    await TestBed.configureTestingModule({
      imports: [CheckoutComponent, FormsModule],
      providers: [
        { provide: CartService, useValue: mockCartService },
        { provide: OrderService, useValue: mockOrderService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CheckoutComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('isFormValid', () => {
    it('should return false when fields are empty', () => {
      component.orderData = { shippingAddress: '', city: '', postalCode: '', country: '', items: [] };
      expect(component.isFormValid()).toBeFalse();
    });

    it('should return true when all fields are filled', () => {
      component.orderData = {
        shippingAddress: '123 Main St',
        city: 'New York',
        postalCode: '10001',
        country: 'USA',
        items: []
      };
      expect(component.isFormValid()).toBeTrue();
    });
  });

  describe('placeOrder', () => {
    beforeEach(() => {
      component.orderData = {
        shippingAddress: '123 Main St',
        city: 'New York',
        postalCode: '10001',
        country: 'USA',
        items: mockCartItems
      };
    });

    it('should check authentication first', () => {
      mockAuthService.isAuthenticated.and.returnValue(false);
      spyOn(window, 'alert');
      
      component.placeOrder();
      
      expect(window.alert).toHaveBeenCalledWith('Please login to place order');
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should create order when valid', () => {
      mockAuthService.isAuthenticated.and.returnValue(true);
      mockOrderService.createOrder.and.returnValue(of({}));
      spyOn(window, 'alert');
      
      component.placeOrder();
      
      expect(mockOrderService.createOrder).toHaveBeenCalled();
      expect(mockCartService.clearCart).toHaveBeenCalled();
    });
  });
});