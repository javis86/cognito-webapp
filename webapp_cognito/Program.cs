using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using webapp_cognito;

var builder = WebApplication.CreateBuilder(args);

var awsCognitoOptions = builder.Configuration.GetSection(AwsCognitoOptions.Options)
    .Get<AwsCognitoOptions>();

builder.Services.AddSingleton(awsCognitoOptions);

// Add services to the container.
builder.Services.AddCognitoIdentity();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://cognito-idp.{awsCognitoOptions?.Region}.amazonaws.com/{awsCognitoOptions?.UserPoolId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://cognito-idp.{awsCognitoOptions?.Region}.amazonaws.com/{awsCognitoOptions?.UserPoolId}",
            ValidateLifetime = true,
            LifetimeValidator = (before, expires, token, param) => expires > DateTime.UtcNow,
            ValidateAudience = false,
        };
    });

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/account/create",
        async (IAmazonCognitoIdentityProvider cognitoIdentityProvider,
            AwsCognitoOptions awsCognitoOptions,
            NewUserDataModel userDataModel) =>
        {
            var userAttrsList = new List<AttributeType>()
            {
                new AttributeType(){
                    Name = "email",
                    Value = "pepito@domain.com",
                },
                new AttributeType(){
                    Name = "phone_number",
                    Value = userDataModel.Username,
                },
            };

            var username = Guid.NewGuid().ToString();
            Console.WriteLine($@"username-id {username}");
            
            var signUpRequest = new SignUpRequest
            {
                UserAttributes = userAttrsList,
                Username = username,
                ClientId = awsCognitoOptions.UserPoolClientId,
                Password = userDataModel.Password,
            };

            var response = await cognitoIdentityProvider.SignUpAsync(signUpRequest);

            return response;
        })
    .WithName("AccountCreation")
    .WithOpenApi();


app.MapPost("/account/confirm",
        async (IAmazonCognitoIdentityProvider cognitoIdentityProvider,
            AwsCognitoOptions awsCognitoOptions,
            NewUserDataModel userDataModel) =>
        {
            var signUpRequest = new AdminConfirmSignUpRequest()
            {
                Username = userDataModel.Username,
                UserPoolId = awsCognitoOptions.UserPoolId
            };

            var response = await cognitoIdentityProvider.AdminConfirmSignUpAsync(signUpRequest);

            return response;
        })
    .WithName("AccountConfirmation")
    .WithOpenApi();

app.MapPost("/account/confirmPhoneNumber",
        async (IAmazonCognitoIdentityProvider cognitoIdentityProvider,
            AwsCognitoOptions awsCognitoOptions,
            NewUserDataModel userDataModel) =>
        {
            
            // This method programmatically handles phone number confirmation.
            //  This flow can also be managed within AWS Cognito through its native functionality and/or through its triggers.
            
            var request = new AdminUpdateUserAttributesRequest
            {
                UserPoolId = awsCognitoOptions.UserPoolId,
                Username = userDataModel.Username,
                UserAttributes = new List<AttributeType>
                {
                    new() { Name = "phone_number_verified", Value = "true" }
                }
            };

            try
            {
                var response = await cognitoIdentityProvider.AdminUpdateUserAttributesAsync(request);
                Console.WriteLine($"Successfully updated user attributes for {userDataModel.Username}.");
                
                return response;
            }
            catch (AmazonCognitoIdentityProviderException ex)
            {
                Console.WriteLine($"Error updating user attributes: {ex.Message}");
                throw;
            }
        })
    .WithName("PhoneNumberConfirmation")
    .WithOpenApi();

app.MapPost("/account/login",
        async (IAmazonCognitoIdentityProvider cognitoIdentityProvider,
            AwsCognitoOptions awsCognitoOptions,
            NewUserDataModel userDataModel) =>
        {
            var authParameters = new Dictionary<string, string>
            {
                { "USERNAME", userDataModel.Username },
                { "PASSWORD", userDataModel.Password }
            };

            var authRequest = new AdminInitiateAuthRequest
            {
                ClientId = awsCognitoOptions.UserPoolClientId,
                UserPoolId = awsCognitoOptions.UserPoolId,
                AuthParameters = authParameters,
                AuthFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH,
            };

            var authResponse = await cognitoIdentityProvider.AdminInitiateAuthAsync(authRequest);
            return authResponse;
        })
    .WithName("AccountLogin")
    .WithOpenApi();

app.MapGet("/account/showInfo",
        (IAmazonCognitoIdentityProvider cognitoIdentityProvider,
            AwsCognitoOptions awsCognitoOptions) =>
        {
            return;
        })
    .WithName("ShowInfo")
    .RequireAuthorization()
    .WithOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

public class NewUserDataModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}