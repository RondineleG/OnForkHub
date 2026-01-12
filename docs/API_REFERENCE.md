# 📚 API Reference Guide

Complete API documentation for OnForkHub endpoints and services.

---

## Table of Contents

- [Base URL](#base-url)
- [Authentication](#authentication)
- [Categories API](#categories-api)
- [Error Handling](#error-handling)
- [Response Format](#response-format)
- [Rate Limiting](#rate-limiting)

---

## Base URL

```
Production:   https://api.onforkhub.com
Development:  http://localhost:5000
Staging:      http://172.245.152.43:9000
```

---

## Authentication

### JWT Bearer Token

All endpoints (except public endpoints) require authentication via JWT Bearer token.

```http
Authorization: Bearer <your_jwt_token>
```

### Obtaining a Token

```bash
# Login endpoint
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "your_password"
}

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "refreshToken": "refresh_token_here"
}
```

### Refresh Token

```bash
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "your_refresh_token"
}
```

---

## Categories API

### Base Path: `/api/v1/categories`

### List Categories

**Endpoint:**
```
GET /api/v1/categories
```

**Parameters:**
- `page` (query, integer): Page number (default: 1)
- `pageSize` (query, integer): Items per page (default: 10, max: 100)
- `search` (query, string): Search term for filtering

**Example Request:**
```bash
curl -X GET "http://localhost:5000/api/v1/categories?page=1&pageSize=20&search=music" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

**Success Response (200 OK):**
```json
{
  "data": [
    {
      "id": "507f1f77bcf86cd799439011",
      "name": "Music",
      "description": "Music videos and content",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z",
      "isActive": true
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 45,
    "totalPages": 3
  },
  "statusCode": 200,
  "succeeded": true
}
```

**Error Response (401 Unauthorized):**
```json
{
  "message": "Authorization header missing or invalid",
  "statusCode": 401,
  "succeeded": false
}
```

---

### Get Category by ID

**Endpoint:**
```
GET /api/v1/categories/{id}
```

**Parameters:**
- `id` (path, string): Category ID (MongoDB ObjectId)

**Example Request:**
```bash
curl -X GET "http://localhost:5000/api/v1/categories/507f1f77bcf86cd799439011" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Success Response (200 OK):**
```json
{
  "data": {
    "id": "507f1f77bcf86cd799439011",
    "name": "Music",
    "description": "Music videos and content",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "isActive": true
  },
  "statusCode": 200,
  "succeeded": true
}
```

**Error Response (404 Not Found):**
```json
{
  "message": "Category not found",
  "statusCode": 404,
  "succeeded": false
}
```

---

### Create Category

**Endpoint:**
```
POST /api/v1/categories
```

**Request Body:**
```json
{
  "name": "Technology",
  "description": "Tech-related videos",
  "isActive": true
}
```

**Example Request:**
```bash
curl -X POST "http://localhost:5000/api/v1/categories" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Technology",
    "description": "Tech-related videos",
    "isActive": true
  }'
```

**Success Response (201 Created):**
```json
{
  "data": {
    "id": "507f1f77bcf86cd799439012",
    "name": "Technology",
    "description": "Tech-related videos",
    "createdAt": "2024-01-15T10:35:00Z",
    "updatedAt": "2024-01-15T10:35:00Z",
    "isActive": true
  },
  "statusCode": 201,
  "succeeded": true
}
```

**Validation Error Response (400 Bad Request):**
```json
{
  "errors": [
    {
      "field": "name",
      "message": "Category name must not be empty"
    }
  ],
  "statusCode": 400,
  "succeeded": false
}
```

---

### Update Category

**Endpoint:**
```
PUT /api/v1/categories/{id}
```

**Parameters:**
- `id` (path, string): Category ID

**Request Body:**
```json
{
  "name": "Technology",
  "description": "Updated description",
  "isActive": true
}
```

**Example Request:**
```bash
curl -X PUT "http://localhost:5000/api/v1/categories/507f1f77bcf86cd799439011" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Technology",
    "description": "Updated description",
    "isActive": true
  }'
```

**Success Response (200 OK):**
```json
{
  "data": {
    "id": "507f1f77bcf86cd799439011",
    "name": "Technology",
    "description": "Updated description",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:40:00Z",
    "isActive": true
  },
  "statusCode": 200,
  "succeeded": true
}
```

---

### Delete Category

**Endpoint:**
```
DELETE /api/v1/categories/{id}
```

**Parameters:**
- `id` (path, string): Category ID

**Example Request:**
```bash
curl -X DELETE "http://localhost:5000/api/v1/categories/507f1f77bcf86cd799439011" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Success Response (204 No Content):**
```
HTTP/1.1 204 No Content
```

**Error Response (409 Conflict):**
```json
{
  "message": "Cannot delete category with associated videos",
  "statusCode": 409,
  "succeeded": false
}
```

---

## Error Handling

### Error Response Format

All errors follow a consistent format:

```json
{
  "message": "Human-readable error message",
  "statusCode": 400,
  "succeeded": false,
  "errors": [
    {
      "field": "fieldName",
      "message": "Field-specific error message"
    }
  ],
  "traceId": "0HN1GJ7IV4QFS:00000001"
}
```

### HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET request |
| 201 | Created | Successful POST request |
| 204 | No Content | Successful DELETE request |
| 400 | Bad Request | Invalid input data |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | Authenticated but lacks permission |
| 404 | Not Found | Resource does not exist |
| 409 | Conflict | Request conflicts with current state |
| 422 | Unprocessable Entity | Validation failed |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |
| 503 | Service Unavailable | Service temporarily unavailable |

---

## Response Format

### Success Response

```json
{
  "data": {},
  "statusCode": 200,
  "succeeded": true
}
```

### Paginated Response

```json
{
  "data": [],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "statusCode": 200,
  "succeeded": true
}
```

### Error Response

```json
{
  "message": "Error description",
  "statusCode": 400,
  "succeeded": false
}
```

---

## Rate Limiting

### Limits

- **Default:** 1000 requests per hour
- **Authenticated:** 5000 requests per hour
- **Premium:** Unlimited

### Headers

Response headers include rate limit information:

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1623456789
```

### Exceeding Rate Limit

```json
{
  "message": "Rate limit exceeded. Try again in 60 seconds.",
  "statusCode": 429,
  "succeeded": false,
  "retryAfter": 60
}
```

---

## Common Request Patterns

### Pagination Example

```bash
# Get page 2 with 25 items per page
curl "http://localhost:5000/api/v1/categories?page=2&pageSize=25"
```

### Search Example

```bash
# Search for categories containing "music"
curl "http://localhost:5000/api/v1/categories?search=music"
```

### Sorting Example

```bash
# Sort by creation date descending
curl "http://localhost:5000/api/v1/categories?sortBy=createdAt&sortOrder=desc"
```

### Filtering Example

```bash
# Get only active categories
curl "http://localhost:5000/api/v1/categories?isActive=true"
```

---

## SDK Examples

### cURL

```bash
# List categories
curl -H "Authorization: Bearer TOKEN" \
  http://localhost:5000/api/v1/categories
```

### JavaScript/TypeScript

```typescript
const response = await fetch('http://localhost:5000/api/v1/categories', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const data = await response.json();
```

### Python

```python
import requests

headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

response = requests.get(
    'http://localhost:5000/api/v1/categories',
    headers=headers
)

data = response.json()
```

### C# / .NET

```csharp
using HttpClient client = new();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

var response = await client.GetAsync(
    "http://localhost:5000/api/v1/categories");

var data = await response.Content.ReadAsAsync<ResponseDto>();
```

---

## Webhooks

### Webhook Events

- `category.created` - New category created
- `category.updated` - Category updated
- `category.deleted` - Category deleted

### Webhook Payload

```json
{
  "event": "category.created",
  "timestamp": "2024-01-15T10:35:00Z",
  "data": {
    "id": "507f1f77bcf86cd799439012",
    "name": "Technology",
    "description": "Tech-related videos"
  }
}
```

---

## API Versioning

The API supports versioning through URL paths:

- `v1` - Current stable version
- `v2` - Upcoming version (beta)

```
GET /api/v1/categories      # Use v1 (stable)
GET /api/v2/categories      # Use v2 (beta)
```

---

## Support

- **API Status:** https://status.onforkhub.com
- **API Docs:** https://api.onforkhub.com/swagger
- **Support Email:** support@onforkhub.com
- **Discord:** https://discord.gg/onforkhub

---

*Last Updated: 2025-11-07*
*API Version: 1.0.0*
