# SAML2
Fork of [SAML2 library](https://github.com/anthonylangsworth/SAML2). This library removes dependencies on Asp.net


This project now consists of three libraries

1. SAML2.Core: This contains all the core logic from the original SAML2 library on codeplex and remains similar in style and structure. Configuration has been changed to no longer be married to System.Configuration. Filewatchers on metadata were problematic and have been removed on the belief that this additional functionality can be provided outside the core library
1. SAML2.AspNet: This contains all the ASP.Net bits from the original library including the configuration. This has not been tested, but theoretically SAML2.AspNet + Saml2.Core should be equivalent to the original single library on codeplex (minus the filewatchers)
1. Owin.Security.Saml: This contains an OWIN middleware implementation of SAMLP Service Provider. This library is the main driver for this effort.
1. SelfHostOwinSPExample: Example using simple-idp an nodejs SAML2  Identity Provider Implementation

## Install a test IDP usin node (needs nodejs and openssl)
- clon [this](https://github.com/mcguinness/saml-idp) git repository.
- ```npm install```
- ```openssl req -x509 -new -newkey rsa:2048 -nodes -subj '/C=US/ST=California/L=San Francisco/O=JankyCo/CN=Test Identity Provider' -keyout idp-private-key.pem -out idp-public-cert.pem -days 7300```
- ```node app.js --acs http://localhost:7777/identity --aud uri:pqs```
- 