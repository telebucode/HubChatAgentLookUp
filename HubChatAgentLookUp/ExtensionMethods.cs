using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubChatAgentLookUp
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Determines whether the object is equalant to DbNull.Value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        ///   <c>true</c> if the input is equalant to DbNull.Value then <c>true;</c> otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsDbNull(this object input)
        {
            return input.Equals(DBNull.Value) ? true : false;
        }
        /// <summary>
        /// Determines whether SqlCommand operation is success.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        /// <returns>
        ///   <c>true</c> if the specified SQL command is success; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Success output parameter is not found in the sqlcommand parameters list.</exception>
        /// <exception cref="InvalidDataException">Success output parameter has value DbNull</exception>
        internal static bool IsSuccess(this System.Data.SqlClient.SqlCommand sqlCommand)
        {
            if (!sqlCommand.Parameters.Contains(ProcedureParameter.SUCCESS))
                throw new KeyNotFoundException("Success output parameter is not found in the sqlcommand parameters collection.");
            if (sqlCommand.Parameters[ProcedureParameter.SUCCESS].IsDbNull())
                throw new InvalidDataException("Success output parameter has value DbNull");
            return Convert.ToBoolean(sqlCommand.Parameters[ProcedureParameter.SUCCESS].Value);
        }
        /// <summary>
        /// Gets the Message output parameter value from SqlCommand object.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        /// <returns>The value of the Message output parameter</returns>
        /// <exception cref="KeyNotFoundException">Message output parameter is not found in the sqlcommand parameters collection</exception>
        internal static string GetMessage(this System.Data.SqlClient.SqlCommand sqlCommand)
        {
            if (!sqlCommand.Parameters.Contains(ProcedureParameter.MESSAGE))
                throw new KeyNotFoundException("Message output parameter is not found in the sqlcommand parameters collection");
            return sqlCommand.Parameters[ProcedureParameter.MESSAGE].IsDbNull() ? "Null Message" : sqlCommand.Parameters[ProcedureParameter.MESSAGE].Value.ToString();
        }

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        ///   <c>true</c> if the specified input is empty; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsEmpty(this string input)
        {
            return input.IsDbNull() || input == null || input.Trim().Length == 0;
        }
        internal static bool IsSuccess(this JObject jobject)
        {
            bool result = false;
            if (jobject.SelectToken(Label.SUCCESS) != null)
                bool.TryParse(jobject.SelectToken(Label.SUCCESS).ToString(), out result);
            return result;
        }
        internal static byte ToByte(this string input, byte ifEmptyOrFailed)
        {
            byte output = ifEmptyOrFailed;
            output = (!input.IsEmpty() && byte.TryParse(input, out output)) ? output : ifEmptyOrFailed;
            return output;
        }
        internal static long ToLong(this string input, long ifEmptyOrFailed)
        {
            long output = ifEmptyOrFailed;
            output = (!input.IsEmpty() && long.TryParse(input, out output)) ? output : ifEmptyOrFailed;
            return output;
        }
        internal static bool ToBoolean(this string input, bool ifEmptyOrFailed)
        {
            bool output = ifEmptyOrFailed;
            output = (!input.IsEmpty() && bool.TryParse(input, out output)) ? output : ifEmptyOrFailed;
            return output;
        }
        internal static int ToInt(this string input, int ifEmptyOrFailed)
        {
            int output = ifEmptyOrFailed;
            output = (!input.IsEmpty() && int.TryParse(input, out output)) ? output : ifEmptyOrFailed;
            return output;
        }
    }
}
