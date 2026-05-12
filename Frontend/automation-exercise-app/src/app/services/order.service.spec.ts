import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { OrderService } from './order.service';
import { OrderRequest } from '../models/cart.model';

describe('OrderService', () => {
  let service: OrderService;
  let httpMock: HttpTestingController;
  const apiUrl = 'https://localhost:7243/api/orders';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [OrderService]
    });
    service = TestBed.inject(OrderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('createOrder', () => {
    const mockOrderRequest: OrderRequest = {
      shippingAddress: '123 Main St',
      city: 'New York',
      postalCode: '10001',
      country: 'USA',
      items: [
        { productId: 1, productName: 'Product 1', price: 100, quantity: 2, totalPrice: 200 }
      ]
    };

    const mockOrderResponse = {
      id: 1,
      userId: 1,
      totalAmount: 200,
      status: 'Pending'
    };

    it('should create an order', () => {
      service.createOrder(mockOrderRequest).subscribe(response => {
        expect(response).toEqual(mockOrderResponse);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('POST');
      req.flush(mockOrderResponse);
    });
  });

  describe('getUserOrders', () => {
    const mockOrders = [
      { id: 1, totalAmount: 100, status: 'Pending' },
      { id: 2, totalAmount: 200, status: 'Completed' }
    ];

    it('should get user orders', () => {
      service.getUserOrders().subscribe(orders => {
        expect(orders).toEqual(mockOrders);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockOrders);
    });
  });

  describe('getOrderById', () => {
    const mockOrder = { id: 1, totalAmount: 100, status: 'Pending' };

    it('should get order by id', () => {
      service.getOrderById(1).subscribe(order => {
        expect(order).toEqual(mockOrder);
      });

      const req = httpMock.expectOne(`${apiUrl}/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockOrder);
    });
  });
});