namespace Erkon.Models
{
    public class Successful
    {
        private readonly string _identifier;
        public Successful(string identifier, string messageType)
        {
            _identifier = identifier;
            Message = SuccessfulMessage(messageType);
        }
        public string Message { get; set; }
        public string SuccessfulMessage(string messageType)
        {
            switch (messageType)
            {
                case "addunit":
                    return $"New unit has been added successfully. The assigned unit Id is {_identifier}. Please use the Id as reference for Microcontroller.";
                case "inactivateunit":
                    return $"Unit with code {_identifier} has been inactivated successfully.";
                case "editunit":
                    return $"Unit with code {_identifier} has been modified successfully.";
                case "addaccess":
                    return $"The access has been added successfully.";
                case "editaccess":
                    return $"The access has been modified successfully.";
                case "inactivateaccess":
                    return $"The access has been inactivated successfully.";
                default:
                    return "Undefined";
            }
        }
    }
}
