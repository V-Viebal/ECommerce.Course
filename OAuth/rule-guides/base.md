# Project Style Guide
**Last updated on 2025-06-08.**
This rule guide ensures consistency, readability, and maintainability across the project. It is designed to be lean and practical, focusing on essential practices for clean code at backend perspective.

## Table of Contents
- [Structure](#structure)
- [Introduction](#introduction)
- [General Principles](#general-principles)
- [General Architecture Constrainsts](#general-architecture-constrainsts)
- [Language-Specific Guidelines](#language-specific-guidelines)
- [Tools](#tools)

## Structure
The project follows Clean Architecture and Domain Driven Design, with each folders having its own responsibility.

## Project Structure
```bash
src/
├── Contracts/          # Contract project prepares to share for public APIs.
├── Domain/             # Domain project provides the core entity, business model,...
├── SharedKernel/       # Shared project provides common components, services, interfaces, utilities, and nuget package directives.
├── UseCase/            # UseCase project provides the business logic, work flow of the app.
├── Infrastructure/     # Infrastructure project provides the infrastructure of the app such as service implementation, third party integration, data persistant,...
├── API/                # API project provides the Restful API of the app.
tests/                  # Tests projects for IT and UT
rule-guides/            # Global rules for coding and conventions at C# asp.net core >=8.
```

## Introduction
This style guide provides coding standards and best practices for all developers working on this project. Following these guidelines ensures that the codebase remains clean, consistent, easy to maintain and scale in the future.

## General Principles
- **DRY (Don't Repeat Yourself)**: MUST avoid duplicating code.
- **KISS (Keep It Simple, Stupid)**: MUST prefer simple solutions over complex ones.
- **YAGNI (You Aren't Gonna Need It)**: MUST only implement features that are necessary.
- **SOLID**: MUST follow these principles to code with Object Orient Programing. Especially, Single Responsibility PrincipleEach to ensure module or class MUST have one clear responsibility.
- **Consistent Naming**: MUST use meaningful and consistent names for variables, functions, and classes.
- **Commenting**: SHOULD add comments for complex logic or non-obvious code.

### General Architecture Constrainsts
For forcing the developer to program the correct direction of the Clean Architecture and Domain Driven Design, the following rules MUST be followed:

- **Project Dependency**: 
  - Domain only reference to Contract as optional.
  - Contract NOT reference to any other projects.
  - UseCase only reference to Domain or Contract (optional).
  - Infrastructure only reference to UseCase.
  - API only reference to UseCase and Infrastructure.
  - SharedKernel NOT reference to any other projects.
  - SharedKernel is only referenced by UseCase.

- **Package Dependency**:
  - All nuget package will be introduced from SharedKernel as transition to those projects where has a reference to SharedKernel.
  - To those public app packages where want to enforce the integration app for version compatible, they should define it explicitly.

- **Domain**:
  - MUST use Ubiquitous Language for naming the business models.
  - MUST separate the business model and bounded context strictly.
  - MUST use DDD Aggregate Root as the main entity of the bounded context.
  - MUST using ValueObjects if needed.
  - MUST avoid direct dependency between contexts by using Domain Events,...

- **Functions**:
  - MUST create a helper function for the logic and may reuse within the same bounded context.
  - MUST create a utility function for the logic that can be reused across the bounded context.
  - MUST create a provider function for the complex logic that can be reused across the bounded context.
  - MUST create a service function for the complex logic to adopt the business logic that is also be reused accross the bounded context.
  - MUST create a factory for the object initialization that can be reused across the bounded context.

- **Testing**:
  - MUST follow the AAA pattern for testing.

## Language-Specific Guidelines

- **Language and SDK**:
  - MUST use the advance features of the current version of the C# language.
  - MUST use the advance features of the current version of the ASP.NET Core.

- **Build**:
  - MUST dotnet clean before dotnet build.

- **Code Formatting**:
  - MUST follow the editor formatting configuration.

## Tools

- **Code Analysis**:
  - MUST follow the SonarQube rules.