using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mautom.Portunus.Entities.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Mautom.Portunus.Formatters
{
    public class HkpOutputFormatter : TextOutputFormatter
    {
        public HkpOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));
            
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(PublicKeyDto).IsAssignableFrom(type) || typeof(IEnumerable<PublicKeyDto>).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }

            return false;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();

            var count = 1;
            
            if (context.Object is IEnumerable<PublicKeyDto> keys)
                count = keys.Count();

            buffer.AppendLine($"info:1:{count}");

            if (context.Object is IEnumerable<PublicKeyDto> publicKeys)
            {
                foreach (var key in publicKeys)
                {
                    FormatHkpPublicKey(buffer, key);
                }
            }
            else
            {
                FormatHkpPublicKey(buffer, (PublicKeyDto)context.Object);
            }

            await response.WriteAsync(buffer.ToString());
        }

        public static void FormatHkpPublicKey(StringBuilder buffer, PublicKeyDto key)
        {
            buffer.AppendLine(key.ToString("HKP"));

            foreach (var identity in key.KeyIdentities)
                buffer.AppendLine(identity.ToString("HKP"));
            
        }
    }
}