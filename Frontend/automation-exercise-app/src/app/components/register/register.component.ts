import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest } from '../../models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerData: RegisterRequest = {
    username: '',
    email: '',
    password: '',
    firstName: '',
    lastName: ''
  };
  isLoading = false;
  confirmPassword = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    if (this.registerData.password !== this.confirmPassword) {
      alert('Passwords do not match!');
      return;
    }

    if (this.registerData.password.length < 6) {
      alert('Password must be at least 6 characters long!');
      return;
    }

    this.isLoading = true;
    this.authService.register(this.registerData).subscribe({
      next: () => {
        this.router.navigate(['/products']);
      },
      error: (error) => {
        console.error('Registration error:', error);
        alert('Registration failed: ' + (error.error?.message || 'Unknown error'));
        this.isLoading = false;
      }
    });
  }
}