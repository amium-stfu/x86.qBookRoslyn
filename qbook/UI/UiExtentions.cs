using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbook.UI
{
    public static class UiExtentions
    {
        public static bool ShowEditTextDialog(ref string s, string text = "")
        {
            string localValue = s;

            Keyboard keyboard = new Keyboard(
               getter: () => localValue,
               setter: (val) => localValue = val,
               text: text
           );

            keyboard.Qertz();
            keyboard.ShowNumblock();
            keyboard.ShowDialog();

            // Update the ref parameter after dialog closes
            s = localValue;
            return keyboard.DialogResult == System.Windows.Forms.DialogResult.OK;
        }
        public static void ShowEditDialog(this string v, ref string value, string unit1 = "", string unit2 = "", string unit3 = "", string unit4 = "", string text = "")
        {
            string localValue = value;

            // Pass the local variable to NumBlock
            NumBlock edit = new NumBlock(
                getter: () => localValue,
                setter: (val) => localValue = val,
                text: text,
                unit1: unit1,
                unit2: unit2,
                unit3: unit3,
                unit4: unit4
            );

            edit.ShowDialog();

            // Update the reference parameter after dialog closes
            value = localValue;
        }
        public static void ShowEditDialog(ref this int v, string text = "", string unit = "", string unit2 = "", string unit3 = "", string unit4 = "", int min = int.MinValue, int max = int.MaxValue)
        {
            {
                int localValue = v;

                // Pass the local variable to NumBlock
                NumBlock edit = new NumBlock(
                    getter: () => localValue,
                    setter: (value) => localValue = value,
                    cText: text,
                    cUnit: unit,
                    cMax: max,
                    cMin: min

                );

                edit.ShowDialog();

                // Update the ref parameter after dialog closes
                v = localValue;

            }
        }
        //
    }
}
