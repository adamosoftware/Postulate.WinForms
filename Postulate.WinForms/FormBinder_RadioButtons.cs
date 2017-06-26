﻿using Postulate.Orm.Abstract;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace Postulate.WinForms
{
    public partial class FormBinder<TRecord, TKey> where TRecord : Record<TKey>, new()
    {
        public void AddControl(RadioButton control, Action<TRecord> setProperty, Action<TRecord> setControl, bool defaultValue = false)
        {
            control.CheckedChanged += delegate (object sender, EventArgs e) { ValueChanged(setProperty); };
            _setControls.Add(setControl);
            _setDefaults.Add(() => { control.Checked = defaultValue; });
        }

        public void AddRadioButtons<TValue>(RadioButtonDictionary<TValue> radioButtons, Expression<Func<TRecord, TValue>> property, TValue defaultValue = default(TValue))
        {
            PropertyInfo pi = GetProperty(property);

            foreach (var rbb in radioButtons)
            {
                Action<TRecord> setProperty = (record) =>
                {
                    pi.SetValue(record, rbb.Key);
                };

                var func = property.Compile();
                Action<TRecord> setControl = (record) =>
                {
                    rbb.Value.Checked = func.Invoke(record).Equals(rbb.Key);
                };

                bool innerDefault = defaultValue?.Equals(rbb.Key) ?? false;
                AddControl(rbb.Value, setProperty, setControl, innerDefault);
            }
        }
    }
}