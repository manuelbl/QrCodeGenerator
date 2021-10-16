/* 
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 * 
 * Deom creating a QR code containing a vCard.
 *
 */
using MixERP.Net.VCards;
using MixERP.Net.VCards.Models;
using MixERP.Net.VCards.Serializer;
using MixERP.Net.VCards.Types;
using Net.Codecrete.QrCodeGenerator;
using System.Collections.Generic;
using System.IO;

namespace VCardDemo
{
    class Program
    {
        static void Main()
        {
            var vcard = new VCard
            {
                Version = VCardVersion.V3,
                FirstName = "Robin",
                LastName = "Hood",
                Organization = "Sherwood Inc.",
                Addresses = new List<Address>
                {
                    new Address {
                        Type = AddressType.Work,
                        Street = "The Major Oak",
                        Locality = "Sherwood Forest",
                        PostalCode = "NG21 9RN",
                        Country = "United Kingdom",
                    }
                },
                Telephones = new List<Telephone>
                {
                    new Telephone {
                        Type = TelephoneType.Work,
                        Number = "+441623677321"
                    }
                },
                Emails = new List<Email>
                {
                    new Email
                    {
                        Type = EmailType.Smtp,
                        EmailAddress = "robin.hood@sherwoodinc.co.uk"
                    }
                }
            };

            var qrCode = QrCode.EncodeText(vcard.Serialize(), QrCode.Ecc.Medium);
            File.WriteAllText("vcard-qrcode.svg", qrCode.ToSvgString(3));
        }
    }
}
