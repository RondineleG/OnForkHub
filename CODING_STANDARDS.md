# 📖 Coding Standards & Clean Code Guide

This guide establishes coding standards and best practices to maintain clean, organized, and sustainable code in the project.

---

## 1. Naming Conventions

### 1.1. Classes
- Use descriptive names in **PascalCase**.
    - **Example:** `UserService`, `ProductController`.
- The name should clearly reflect the class's purpose.

### 1.2. Methods
- Use **PascalCase** and verbs to indicate the action performed.
    - **Example:** `CalculateTotal()`, `GetUserById(int id)`.

### 1.3. Variables
- Use **camelCase**.
- Choose descriptive names, avoiding obscure abbreviations.
    - **Example:** `orderCount`, `userName`.

### 1.4. Constants
- Use **UPPER_CASE_SNAKE**.
    - **Example:** `MAX_EXECUTION_TIME = 30;`.

### 1.5. Properties
- Use **PascalCase**.
    - **Example:** `public string Name { get; set; }`.

---

## 2. Code Structure

### 2.1. Folder Organization
- Organize files by **responsibility** and maintain logical grouping.
- Avoid deeply nested structures to improve discoverability.
- Follow existing project conventions instead of reinventing structure.


### 2.2. Class Structure
1. **Private fields.**
2. **Public properties.**
3. **Constructors.**
4. **Public methods.**
5. **Private methods.**

---

## 3. Methods

### 3.1. Size
- Methods should not exceed **15 lines**.
- A method should handle **a single responsibility** (SRP).

### 3.2. Naming
- The name should reflect the action and purpose of the method.
    - **Bad:** `Process()`.
    - **Good:** `ProcessPayment(Order order)`.

### 3.3. Parameters
- Avoid methods with more than 3 parameters.
- Consider using objects to encapsulate complex parameters.

---

## 4. Clean Code Practices

### 4.1. Comments
- Favor readable code over explanatory comments.
- Use **XML comments** only for public methods.
  ```csharp
  /// <summary>
  /// Calculates the total order amount with discounts.
  /// </summary>
  /// <param name="order">Order data.</param>
  /// <returns>Total amount with discount.</returns>
  double CalculateTotal(Order order);
  ```

### 4.2. Error Handling
- Handle exceptions specifically.
  ```csharp
  try {
      // Code
  } catch (SqlException ex) {
      Logger.Log(ex.Message);
  }
  ```

### 4.3. Variable Usage
- Prefer `var` when the type is obvious:
  ```csharp
  var orders = new List<Order>();
  ```

### 4.4. Line Breaks
- Leave a blank line between related code blocks.

---

## 5. Design Patterns
- Use patterns such as:
    - **Repository**: To abstract database access.
    - **Factory**: For creating complex objects.
    - **Singleton**: For unique objects across the project.
- Document where and why each pattern is applied.

---

## 6. Tools and Automation

### 6.1. StyleCop
- Configure **StyleCop** to check:
    - Naming conventions.
    - Class structure.
    - Formatting rules.

### 6.2. Code Analyzers
- Use **SonarQube** or another tool to assess code quality.

### 6.3. CI/CD
- Include style and quality checks in the pipeline.

---

## 7. Code Reviews
- Before merging changes:
    1. Check naming and structure.
    2. Ensure methods follow best practices.
    3. Confirm that the code is **readable and maintainable**.

---

