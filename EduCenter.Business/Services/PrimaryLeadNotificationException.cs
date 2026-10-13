using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCenter.Business.Services
{
    public sealed class PrimaryLeadNotificationException : Exception
    {
        public PrimaryLeadNotificationException(string message) : base(message)
        {

        }
    }
}
