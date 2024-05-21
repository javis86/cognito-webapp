# AWS Cognito WebApp - Admin Flow (Server Side flow)
***

## Cognito Configurations for project
- No Cognito MFA flow. Admin Confirmation.

## Requirements

- AWS credentials for SDK (Userkey, UserSecret) from user who has access to cognito resources
- Cognito application parameters (Region, UserPoolClientId, UserPoolId)
````json
{
  "AWS": {
    "Region": "<< Region >>",
    "UserPoolClientId": "<< Client Id>>",
    "UserPoolId": "<< Pool Id>>"
  }
}
````

## Flow
Signup -> Confirmation -> Login -> Endpoint authorization

### Signup
Endpoint: '/account/create'

### Confirmation
Endpoint: '/account/confirm'

### Login
Endpoint: '/account/login'

