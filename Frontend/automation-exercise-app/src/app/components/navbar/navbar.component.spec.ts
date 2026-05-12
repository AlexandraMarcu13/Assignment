import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';

import { NavbarComponent } from './navbar.component';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;

  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockCartService: jasmine.SpyObj<CartService>;

  let router: Router;

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj(
      'AuthService',
      ['logout'],
      {
        currentUser$: of(null)
      }
    );

    mockCartService = jasmine.createSpyObj(
      'CartService',
      [],
      {
        cartCount$: of(0)
      }
    );

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: mockAuthService },
        { provide: CartService, useValue: mockCartService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    spyOn(router, 'navigate');

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('logout', () => {
    it('should call authService.logout', () => {
      component.logout();

      expect(mockAuthService.logout).toHaveBeenCalled();
    });

    it('should navigate to login', () => {
      component.logout();

      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('toggleMenu', () => {
    it('should toggle menuOpen state', () => {
      expect(component.menuOpen).toBeFalse();

      component.toggleMenu();
      expect(component.menuOpen).toBeTrue();

      component.toggleMenu();
      expect(component.menuOpen).toBeFalse();
    });
  });
});