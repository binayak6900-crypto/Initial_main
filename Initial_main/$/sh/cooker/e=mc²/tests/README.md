# mc² Test Suite

This directory contains comprehensive tests for the mc² compiler and language features.

## Test Categories

### Unit Tests
- Test specific functions and components
- Validate individual features work correctly
- Test edge cases and error conditions

### Property-Based Tests
- Test universal properties across all inputs
- Validate correctness properties from design document
- Use property testing frameworks

### Integration Tests
- Test complete compilation pipeline
- Validate .e² file generation and execution
- Test self-hosting capability

### Assembly Tests
- Test core assembly functions (print, input, memory management)
- Validate spacetime assembly generation
- Test .e² binary format

## Test Structure (To Be Implemented)

```
tests/
├── unit/              # Unit tests
│   ├── lexer/
│   ├── parser/
│   ├── typechecker/
│   └── codegen/
├── property/          # Property-based tests
│   ├── parse-print.test
│   ├── type-inference.test
│   └── semantic-preservation.test
├── integration/       # Integration tests
│   ├── compile-execute.test
│   └── self-host.test
├── assembly/          # Assembly tests
│   ├── print.test
│   ├── input.test
│   └── memory.test
└── fixtures/          # Test fixtures and sample programs
```

## Running Tests

Tests will be run using the mc² test framework (to be implemented) or standard testing tools during bootstrap phase.

## Critical Testing Phase

Before deleting C++ files (Phase 4), ALL assembly tests must pass consistently to ensure the foundation is solid.
