import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';

import { RegisterComponent } from './register.component';
import { AuthService } from '../../services/auth.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['register']);

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      component.registerData = {
        username: 'testuser',
        email: 'test@test.com',
        password: 'password123',
        firstName: 'Test',
        lastName: 'User'
      };
    });

    it('should show alert if passwords do not match', () => {
      spyOn(window, 'alert');

      component.confirmPassword = 'different';

      component.onSubmit();

      expect(window.alert).toHaveBeenCalledWith(
        'Passwords do not match!'
      );

      expect(mockAuthService.register).not.toHaveBeenCalled();
    });

    it('should show alert if password is too short', () => {
      spyOn(window, 'alert');

      component.registerData.password = '12345';
      component.confirmPassword = '12345';

      component.onSubmit();

      expect(window.alert).toHaveBeenCalledWith(
        'Password must be at least 6 characters long!'
      );

      expect(mockAuthService.register).not.toHaveBeenCalled();
    });

    it('should call register service with valid data', () => {
      mockAuthService.register.and.returnValue(of({} as any));

      component.confirmPassword = 'password123';

      component.onSubmit();

      expect(mockAuthService.register).toHaveBeenCalledWith(
        component.registerData
      );
    });

    it('should navigate to products on successful registration', () => {
      mockAuthService.register.and.returnValue(of({} as any));

      component.confirmPassword = 'password123';

      component.onSubmit();

      expect(router.navigate).toHaveBeenCalledWith(['/products']);
    });
  });
});