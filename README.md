# Salesforce-SAML-SSO-ASP-Net
Sample ASP.Net project for Salesforce SAML2 SSO implementation with Sustainsys.Saml2.Owin

This project was originally forked from [Sustainsys SampleOwinApplication](https://github.com/Sustainsys/Saml2/tree/master/Samples/SampleOwinApplication) and adopted to 
the Salesforce SAML SSO. You will need to configure only 3 required variables and 1 optional to make it up and running. The variables are in the 
/App_Start/Start.Auth.cs, but you can move them to Web.Config or any other suitable location for your project.

```
private static string SalesforceIdentityProvider => "https://xxxxxxxx.my.salesforce.com";
private static string ConnectedAppIdentityId => "sample-saml";
private static string MetadataUrl => "https://xxxxxxxx.salesforce.com/.well-known/samlidp/yyyyyy.xml";
private static string SalesforceCertificatePath => HostingEnvironment.MapPath("~/App_Data/SFDC.crt");   // This one is optional
```

## SalesforceIdentityProvider and SalesforceCertificatePath

1. Enable Salesforce as an Identity Provider. Please follow these instructions for domain configuration and Identity Provider setup: https://help.salesforce.com/articleView?id=identity_provider_enable.htm&type=5 
2. After setup is complete use **Issuer** value as `SalesforceIdentityProvider`.
3. This one is optional. If you don't do it then comment `idp.SigningKeys.AddConfiguredKey` line. Download the certificate via the **Download Certificate** button and put it in the App_Data folder. Set `SalesforceCertificatePath` to point to the certificate file.

![Identity Provider Setup](https://raw.githubusercontent.com/alexeybusygin/Salesforce-SAML-SSO-ASP-Net/master/docs/readme-identity-prodiver-setup.png)

## ConnectedAppIdentityId and MetadataUrl

1. In Salesforce, create a connected app.
    1. In Lightning Experience, from Setup, enter App in the Quick Find box, and select App Manager. Click New Connected App.
    2. In Salesforce Classic, from Setup, enter Apps in the Quick Find box, and select Apps. Under Connected Apps, click New.
2. Configure the connected app Basic Information settings.
    1. Enter a name for the connected app (e.g. ASP.Net App). Salesforce uses this name to populate the API Name.
    2. Enter your email address in case Salesforce needs to contact you or your support team.
3. Configure the connected app Web App settings.
    1. Select Enable SAML.
    2. For **Entity Id**, enter `sample-saml` or any other value. User this value for `ConnectedAppIdentityId`.
    3. For **ACS URL**, enter `https://yourdomain/Saml2/Acs`, where `yourdomain` is the application domain.
    4. Select a subject type User ID.
    5. For Name ID Format, keep the default value.
    6. For Issuer, keep the default value.
    7. For IdP Certificate, keep the default (Default IdP Certificate).
    8. Save the settings.
4. Open your connected app in Salesforce for view.
    1. Copy **Metadata Discovery Endpoint** value from the **SAML Login Information** section and put it in the `MetadataUrl` variable.
