# Tools
Collection of useful tools used during developing and debugging.

### JWT.IO
https://jwt.io/

For testing whether JWT structure based on value match what we expect in system.
Paste in the value generated by /auth/login endpoint.

### Bcrypt generator
https://bcrypt-generator.com/

For testing whether passwords match.
For security, use simple and generic passwords for testing purposes only.

## DbDiagram
https://dbdiagram.io/d
For visual representation of database schema.
Paste in entire content of .dbml file.

### EF Migrations
Tool for creating and applying changes to database.
Will be added to CI/CD pipeline later.

### Sonarcloud
https://sonarcloud.io/summary/overall?id=mdabcevic_mk2&branch=main
Static code analysis that highlights code issues that should be fixed before production.
Before each PR approval, all sonarcloud complaints should be taken care of and coverage needs to be sufficient to pass "Quality Gate".

### Docker Desktop
Run the docker compose stack for testing frontend and backend
Build images: docker build -no-cache
Run stack: docker compose up

### Figma
https://www.figma.com/design/9skQgT6qOISLZYrR51RInZ/Coffee?node-id=63-169&p=f&t=KhrI40I0P44M0ckh-0
All our design is contained within this file.
Includes list of screens, reusable components, color schemes and more.

### PgAdmin
An interface for querying postgres databases.

### GitHub Actions
Set of .yml files that define pipelines we run on each branch push and pull requests.
Together they compose our CI/CD pipeline for analyzing, building, testing, deploying solution.

### StreamLit
Python based framework with integrated server and visual components for demo/sketch of product.

### Fork
GUI for git related operations (much like SourceTree)