import apiClient, { unwrap } from './apiClient';

export interface ProductFilterParams {
  search?: string;
  categorySlug?: string;
  brand?: string[];
  minPrice?: number;
  maxPrice?: number;
  sort?: string;
  page?: number;
  pageSize?: number;
  inStock?: boolean;
  [key: string]: unknown;
}

export const catalogApi = {
  home: () =>
    apiClient.get('/catalog/home').then(res => unwrap(res)),

  categories: () =>
    apiClient.get('/catalog/categories').then(res => unwrap(res)),

  filters: (slug?: string) =>
    apiClient.get('/catalog/filters', { params: { slug } }).then(res => unwrap(res)),

  browseProducts: (params: ProductFilterParams) =>
    apiClient.get('/products', { params }).then(res => unwrap(res)),

  getProduct: (slug: string, reviewRating?: number, withCommentOnly?: boolean, reviewPage?: number) =>
    apiClient.get(`/products/${slug}`, { params: { reviewRating, withCommentOnly, reviewPage } })
      .then(res => unwrap(res)),

  getProductReviews: (slug: string, params?: { rating?: number; withCommentOnly?: boolean; page?: number }) =>
    apiClient.get(`/products/${slug}/reviews`, { params }).then(res => unwrap(res)),

  suggestions: (query: string) =>
    apiClient.get('/products/suggestions', { params: { query } }).then(res => unwrap(res)),

  compare: (categoryId: number, ids: string[]) =>
    apiClient.get('/products/compare', { params: { categoryId, ids: ids.join(',') } }).then(res => unwrap(res)),

  storeProfile: (slug: string) =>
    apiClient.get(`/stores/${slug}`).then(res => unwrap(res)),

  storeProducts: (slug: string, params?: ProductFilterParams) =>
    apiClient.get(`/stores/${slug}/products`, { params }).then(res => unwrap(res)),

  followStore: (slug: string) =>
    apiClient.post(`/stores/${slug}/follow`).then(res => unwrap(res)),
};
