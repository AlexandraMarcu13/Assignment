export interface CartItem {
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  totalPrice: number;
}

export interface OrderRequest {
  shippingAddress: string;
  city: string;
  postalCode: string;
  country: string;
  items: CartItem[];
}