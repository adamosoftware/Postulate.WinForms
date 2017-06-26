﻿using Postulate.Orm.Abstract;
using ReflectionHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Postulate.WinForms
{
    public partial class FormBinder<TRecord, TKey> where TRecord : Record<TKey>, new()
    {
        public void AddControl(TextBox control, Expression<Func<TRecord, object>> property, object defaultValue = null)
        {
            AddControl<object>(control, property, defaultValue);
        }

        public void AddControl<TValue>(TextBox control, Expression<Func<TRecord, TValue>> property, TValue defaultValue = default(TValue))
        {
            PropertyInfo pi = GetProperty(property);
            Action<TRecord> writeAction = (record) =>
            {
                pi.SetValue(record, control.Text);
            };

            MaxLengthAttribute attr;
            if (pi.HasAttribute(out attr)) control.MaxLength = attr.Length;

            var func = property.Compile();
            Action<TRecord> readAction = (record) =>
            {
                control.Text = func.Invoke(record)?.ToString();
                _textChanges[control.Name] = false;
            };

            AddControl(control, writeAction, readAction, defaultValue);
        }

        public void AddControl(TextBox control, Action<TRecord> writeAction, Action<TRecord> readAction, object defaultValue = null)
        {
            EventHandler validated = delegate (object sender, EventArgs e)
            {
                if (_textChanges[control.Name])
                {
                    ValueChanged(writeAction);
                    _textChanges[control.Name] = false;
                    _validated[control.Name] = true;
                }
            };

            _textBoxValidators.Add(control.Name, new TextBoxValidator(control, validated));
            _textChanges.Add(control.Name, false);
            control.TextChanged += delegate (object sender, EventArgs e) { if (!_suspend) { _textChanges[control.Name] = true; _validated[control.Name] = false; } };
            control.Validated += validated;
            _readActions.Add(readAction);
            _clearActions.Add(() => { control.Text = defaultValue?.ToString(); });
        }
    }
}