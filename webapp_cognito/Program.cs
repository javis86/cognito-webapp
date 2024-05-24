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
                    Name = "custom:NameIdentifier",
                    Value = Guid.NewGuid().ToString(),
                }, 
            };

            var signUpRequest = new SignUpRequest
            {
                UserAttributes = userAttrsList,
                Username = userDataModel.Username,
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