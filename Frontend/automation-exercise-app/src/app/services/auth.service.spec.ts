import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { LoginRequest } from '../models/user.model';

describe('AuthService', () => {
  let httpMock: HttpTestingController;
  const apiUrl = 'https://localhost:7243/api/auth';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    localStorage.clear();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    const service = TestBed.inject(AuthService);
    expect(service).toBeTruthy();
  });

  describe('isAuthenticated', () => {
    it('should return false when no user', () => {
      localStorage.clear();
      const service = TestBed.inject(AuthService);
      expect(service.isAuthenticated()).toBeFalse();
    });

    it('should return true when user exists', () => {
      const mockUser = {
        id: 1, username: 'test', email: 'test@test.com',
        firstName: '', lastName: '', token: 'token'
      };
      localStorage.setItem('currentUser', JSON.stringify(mockUser));
      const service = TestBed.inject(AuthService);
      expect(service.isAuthenticated()).toBeTrue();
    });
  });

  describe('getToken', () => {
    it('should return null when no user', () => {
      localStorage.clear();
      const service = TestBed.inject(AuthService);
      expect(service.getToken()).toBeNull();
    });

    it('should return token when user exists', () => {
      const mockUser = {
        id: 1, username: 'test', email: 'test@test.com',
        firstName: '', lastName: '', token: 'my-secret-token'
      };
      localStorage.setItem('currentUser', JSON.stringify(mockUser));
      const service = TestBed.inject(AuthService);
      expect(service.getToken()).toBe('my-secret-token');
    });
  });

  describe('login', () => {
    const loginData: LoginRequest = {
      email: 'test@example.com',
      password: 'Test123!'
    };

    const mockResponse = {
      id: 1,
      username: 'testuser',
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
      token: 'fake-jwt-token'
    };

    it('should login user and store token', () => {
      const service = TestBed.inject(AuthService);
      
      service.login(loginData).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/login`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);

      expect(localStorage.getItem('currentUser')).toBeTruthy();
    });
  });

  describe('logout', () => {
    it('should remove user and clear token', () => {
      const mockUser = {
        id: 1, username: 'test', email: 'test@test.com',
        firstName: '', lastName: '', token: 'token'
      };
      
      localStorage.setItem('currentUser', JSON.stringify(mockUser));
      expect(localStorage.getItem('currentUser')).toBeTruthy();
      
      const service = TestBed.inject(AuthService);
      service.logout();
      
      expect(localStorage.getItem('currentUser')).toBeNull();
    });
  });
});