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
Signup -> Confirmation -> Phone Number Confirmation -> Login -> Endpoint authorization

### Signup
Endpoint: '/account/create'

### AutoConfirmation
Endpoint: '/account/confirm'

### Phone Number AutoConfirmation
Endpoint: '/account/confirmPhoneNumber'

### Login
Endpoint: '/account/login'

### Endpoint Authorization requires aws cognito token
Endpoint: /account/ShowInfo

```csharp
app.MapGet("/account/showInfo",
        (IAmazonCognitoIdentityProvider cognitoIdentityProvider,
            AwsCognitoOptions awsCognitoOptions) =>
        {
            return;
        })
    .WithName("ShowInfo")
    .RequireAuthorization()
    .WithOpenApi();
```

