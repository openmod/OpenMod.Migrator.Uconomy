#region

using Rocket.API;

#endregion

namespace fr34kyn01535.Uconomy
{
    public class UconomyConfiguration : IRocketPluginConfiguration
    {
        public string DatabaseAddress;
        public string DatabaseUsername; 
        public string DatabasePassword;
        public int DatabasePort;
        public string DatabaseName;
        public string DatabaseTableName;

        public decimal InitialBalance;

        public string MessageColor;
        public string MoneyName;
        public string MoneySymbol;

        public void LoadDefaults()
        {
            DatabaseAddress = "127.0.0.1";
            DatabaseUsername = "unturned";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabaseTableName = "uconomy";
            DatabasePort = 3306;
            InitialBalance = 30;
            MoneySymbol = "$";
            MoneyName = "Credits";
            MessageColor = "blue";
        }
    }
}