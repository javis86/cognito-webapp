namespace webapp_cognito;

public class AwsCognitoOptions
{
    public string Region { get; set; }

    public string UserPoolClientId { get; set; }

    public string UserPoolClientSecret { get; set; }
        
    public string UserPoolId { get; set; }
    public static string Options => "AWS";
}