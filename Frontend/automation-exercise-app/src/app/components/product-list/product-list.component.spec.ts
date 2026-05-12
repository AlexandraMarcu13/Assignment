import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ProductListComponent } from './product-list.component';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { Product } from '../../models/product.model';

describe('ProductListComponent', () => {
  let component: ProductListComponent;
  let fixture: ComponentFixture<ProductListComponent>;
  let mockProductService: jasmine.SpyObj<ProductService>;
  let mockCartService: jasmine.SpyObj<CartService>;

  const mockProducts: Product[] = [
    {
      id: 1,
      name: 'Product 1',
      description: 'Description 1',
      price: 100,
      category: 'Cat1',
      imageUrl: 'img1.jpg',
      stock: 10
    },
    {
      id: 2,
      name: 'Product 2',
      description: 'Description 2',
      price: 200,
      category: 'Cat2',
      imageUrl: 'img2.jpg',
      stock: 5
    }
  ];

  beforeEach(async () => {
    mockProductService = jasmine.createSpyObj('ProductService', ['getAllProducts']);
    mockCartService = jasmine.createSpyObj('CartService', [
      'addToCart', 'getQuantity', 'updateProductStock', 'getAvailableStock'
    ]);

    await TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [
        { provide: ProductService, useValue: mockProductService },
        { provide: CartService, useValue: mockCartService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('loadProducts', () => {
    it('should load products on init', () => {
      mockProductService.getAllProducts.and.returnValue(of(mockProducts));
      mockCartService.updateProductStock.and.callFake(() => {});

      fixture.detectChanges();

      expect(component.products).toEqual(mockProducts);
      expect(component.loading).toBeFalse();
    });
  });

  describe('addToCart', () => {
    it('should add product to cart', () => {
      spyOn(window, 'alert');
      mockCartService.addToCart.and.returnValue({ success: true, message: 'Added!' });

      component.addToCart(mockProducts[0]);

      expect(mockCartService.addToCart).toHaveBeenCalledWith(mockProducts[0]);
    });
  });

  describe('getRemainingStock', () => {
    it('should return available stock', () => {
      mockCartService.getAvailableStock.and.returnValue(8);
      
      const remaining = component.getRemainingStock(mockProducts[0]);
      
      expect(remaining).toBe(8);
    });
  });

  describe('getQuantityInCart', () => {
    it('should return quantity in cart', () => {
      mockCartService.getQuantity.and.returnValue(3);
      
      const quantity = component.getQuantityInCart(1);
      
      expect(quantity).toBe(3);
    });
  });
});