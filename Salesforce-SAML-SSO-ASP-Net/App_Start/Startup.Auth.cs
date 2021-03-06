﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using SampleOwinApplication.Models;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;
using Sustainsys.Saml2.Owin;
using Sustainsys.Saml2.WebSso;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;

namespace SampleOwinApplication
{
    public partial class Startup
    {
        //
        // Salesforce configuration

        private static string SalesforceIdentityProvider => "https://xxxxxxxx.my.salesforce.com";
        private static string ConnectedAppIdentityId => "sample-saml";
        private static string MetadataUrl => "https://xxxxxxxx.salesforce.com/.well-known/samlidp/yyyyyy.xml";
        private static string SalesforceCertificatePath => HostingEnvironment.MapPath("~/App_Data/SFDC.crt");


        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseSaml2Authentication(CreateSaml2Options());
        }

        private static Saml2AuthenticationOptions CreateSaml2Options()
        {
            var spOptions = CreateSPOptions();
            var Saml2Options = new Saml2AuthenticationOptions(false)
            {
                SPOptions = spOptions
            };

            var idp = new IdentityProvider(new EntityId(SalesforceIdentityProvider), spOptions)
                {
                    AllowUnsolicitedAuthnResponse = true,
                    Binding = Saml2BindingType.HttpRedirect,
                    MetadataLocation = MetadataUrl
                };

            idp.SigningKeys.AddConfiguredKey(new X509Certificate2(SalesforceCertificatePath));

            Saml2Options.IdentityProviders.Add(idp);

            return Saml2Options;
        }

        private static SPOptions CreateSPOptions()
        {
            var swedish = "sv-se";

            var organization = new Organization();
            organization.Names.Add(new LocalizedName("Sustainsys", swedish));
            organization.DisplayNames.Add(new LocalizedName("Sustainsys AB", swedish));
            organization.Urls.Add(new LocalizedUri(new Uri("http://www.Sustainsys.se"), swedish));

            var spOptions = new SPOptions
            {
                EntityId = new EntityId(ConnectedAppIdentityId),
                Organization = organization,
                MinIncomingSigningAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1",
                Logger = new SomeLogger()
            };

            var techContact = new ContactPerson
            {
                Type = ContactType.Technical
            };
            techContact.EmailAddresses.Add("Saml2@example.com");
            spOptions.Contacts.Add(techContact);

            var supportContact = new ContactPerson
            {
                Type = ContactType.Support
            };
            supportContact.EmailAddresses.Add("support@example.com");
            spOptions.Contacts.Add(supportContact);

            var attributeConsumingService = new AttributeConsumingService
            {
                IsDefault = true,
                ServiceNames = { new LocalizedName("Saml2", "en") }
            };

            attributeConsumingService.RequestedAttributes.Add(
                new RequestedAttribute("urn:someName")
                {
                    FriendlyName = "Some Name",
                    IsRequired = true,
                    NameFormat = RequestedAttribute.AttributeNameFormatUri
                });

            attributeConsumingService.RequestedAttributes.Add(
                new RequestedAttribute("Minimal"));

            spOptions.AttributeConsumingServices.Add(attributeConsumingService);

            return spOptions;
        }
    }

    public class SomeLogger : ILoggerAdapter
    {
        public void WriteInformation(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteError(string message, Exception ex)
        {
            Console.WriteLine(message);
        }

        public void WriteVerbose(string message)
        {
            Console.WriteLine(message);
        }
    }
}