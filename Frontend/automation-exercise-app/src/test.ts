import 'zone.js';
import 'zone.js/testing';
import { getTestBed } from '@angular/core/testing';
import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting
} from '@angular/platform-browser-dynamic/testing';

getTestBed().initTestEnvironment(
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting(),
);


import './app/app.component.spec';
import './app/services/cart.service.spec';
import './app/services/auth.service.spec';
import './app/services/order.service.spec';
import './app/services/product.service.spec';
import './app/components/cart/cart.component.spec';
import './app/components/checkout/checkout.component.spec';
import './app/components/login/login.component.spec';
import './app/components/register/register.component.spec';
import './app/components/navbar/navbar.component.spec';
import './app/components/product-list/product-list.component.spec';
import './app/guards/auth.guard.spec';