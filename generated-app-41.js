Goal: Create a professional, functional desktop calculator application for Windows.

Language/Framework: C# with .NET 8 and WPF (Windows Presentation Foundation).

Features and Behavior:
- Standard arithmetic operations: Addition, subtraction, multiplication, and division.
- Numeric keypad input support (0-9) and decimal point.
- Clear (C) button to reset the current calculation.
- Equals (=) button to compute the result.
- A display area that shows the current input and the result of the calculation.
- Support for keyboard input (numeric keys and operation keys).
- Responsive UI that maintains its layout when the window is resized.
- Modern, clean aesthetic consistent with Windows 11 design guidelines.

Inputs/Outputs and Edge Cases:
- Inputs: User clicks on UI buttons or types via keyboard.
- Outputs: Result of calculations displayed in the main text field.
- Edge Cases:
    - Division by zero: Display an error message (e.g., "Cannot divide by zero").
    - Handling floating-point precision issues.
    - Preventing consecutive operators (e.g., "++" or "*/").
    - Empty input attempts on equal operation.

Constraints:
- The implementation should be structured in a maintainable, professional manner (separation of logic and UI).
- Do not use placeholder code; provide a fully functional, production-ready implementation.
- Include clear, professional code comments explaining the logic, especially for the calculation engine.
- Ensure the application handles high-DPI scaling correctly.
- Provide a complete, single-project solution structure compatible with Visual Studio Code.