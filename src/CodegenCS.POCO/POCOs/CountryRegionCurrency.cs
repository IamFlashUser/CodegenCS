﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("CountryRegionCurrency", Schema = "Sales")]
    public partial class CountryRegionCurrency : INotifyPropertyChanged
    {
        #region Members
        private string _countryRegionCode;
        [Key]
        public string CountryRegionCode 
        { 
            get { return _countryRegionCode; } 
            set { SetField(ref _countryRegionCode, value, nameof(CountryRegionCode)); } 
        }
        private string _currencyCode;
        [Key]
        public string CurrencyCode 
        { 
            get { return _currencyCode; } 
            set { SetField(ref _currencyCode, value, nameof(CurrencyCode)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (CountryRegionCode == null && CurrencyCode == null)
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[CountryRegionCurrency]
                (
                    [CountryRegionCode],
                    [CurrencyCode],
                    [ModifiedDate]
                )
                VALUES
                (
                    @CountryRegionCode,
                    @CurrencyCode,
                    @ModifiedDate
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[CountryRegionCurrency] SET
                    [CountryRegionCode] = @CountryRegionCode,
                    [CurrencyCode] = @CurrencyCode,
                    [ModifiedDate] = @ModifiedDate
                WHERE
                    [CountryRegionCode] = @CountryRegionCode AND 
                    [CurrencyCode] = @CurrencyCode";
                conn.Execute(cmd, this);
            }
        }
        #endregion ActiveRecord

        #region Equals/GetHashCode
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            CountryRegionCurrency other = obj as CountryRegionCurrency;
            if (other == null) return false;

            if (CountryRegionCode != other.CountryRegionCode)
                return false;
            if (CurrencyCode != other.CurrencyCode)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (CountryRegionCode == null ? 0 : CountryRegionCode.GetHashCode());
                hash = hash * 23 + (CurrencyCode == null ? 0 : CurrencyCode.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(CountryRegionCurrency left, CountryRegionCurrency right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CountryRegionCurrency left, CountryRegionCurrency right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode

        #region INotifyPropertyChanged/IsDirty
        public HashSet<string> ChangedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public void MarkAsClean()
        {
            ChangedProperties.Clear();
        }
        public virtual bool IsDirty => ChangedProperties.Any();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetField<T>(ref T field, T value, string propertyName) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                ChangedProperties.Add(propertyName);
                OnPropertyChanged(propertyName);
            }
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged/IsDirty
    }
}
