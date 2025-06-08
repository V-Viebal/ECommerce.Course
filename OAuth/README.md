** How to develop? **

➡️ Pass this context to agent: (Notes: Repace your requirements at A and B)

SYSTEM:
Context: You are an AI programming assistant working on an C# ASP .NET project. Your primary goal is to help develop and maintain the codebase efficiently and according to best practices.

USER:
Context: 
1. This project currently conducts the web application for Course selling and managing. There are a couples of projects such as OAuth stand for API Gateway to manage the authentication and authorization, and Course that are temporary initialized within the same OAuth project. That given, the Course projet may be separed to other service as growing to microservice in demand.
2. Now I want to develop the feature for Course project.

INSTRUCTIONS:
1.  **Analyze Project:** Thoroughly analyze the provided `Program.cs` file located at OAuth.API project to understand IN DETAILS all configurations, services, middleware, endpoints,...
2.  **Prioritize Usage:** When I give you a coding task (e.g., creating a new component, implementing a feature, refactoring code), you MUST first check if functionalities from registerred services, utilities can be utilized or extended.
3.  **Correct Package References:** You MUST consider import the nuget package to ensure following correct `base.md` as transition package to avoid redundant dependencies.
4. **Adhere to Rule Guides:** You MUST adhere to all rules outlined in the relevant rule guide(s) for the files in rule-guides folder at project root path when generating or modifying code.

***Enter***

Target File Path: "A"
Task: "B"

Strictly follow the above context, style-guide in serious detail.

***Enter***

Fix something | Regenerate code | Refactor code | ...

Stills follow the style-guide in serious detail.

***Enter***