import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';

import { LoginComponent } from './login.component';
import { AuthService } from '../../services/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['login']);

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('onSubmit', () => {
    it('should show alert if email is empty', () => {
      spyOn(window, 'alert');

      component.loginData = {
        email: '',
        password: 'password'
      };

      component.onSubmit();

      expect(window.alert).toHaveBeenCalledWith(
        'Please enter both email and password'
      );

      expect(mockAuthService.login).not.toHaveBeenCalled();
    });

    it('should call login service with valid credentials', () => {
      mockAuthService.login.and.returnValue(of({} as any));

      component.loginData = {
        email: 'test@test.com',
        password: 'password'
      };

      component.onSubmit();

      expect(mockAuthService.login).toHaveBeenCalledWith(
        component.loginData
      );
    });

    it('should navigate to products on successful login', () => {
      mockAuthService.login.and.returnValue(of({} as any));

      component.loginData = {
        email: 'test@test.com',
        password: 'password'
      };

      component.onSubmit();

      expect(router.navigate).toHaveBeenCalledWith(['/products']);
    });
  });
});