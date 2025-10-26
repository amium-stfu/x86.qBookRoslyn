using System;
using System.Reflection; // to use Missing.Value

namespace qbook
{

    public class OutlookMail
    {

        public static int Read()
        {
            try
            {

                // Create the Outlook application.
                // in-line initialization
                //   Microsoft.Office.Interop.Outlook.Application

                Microsoft.Office.Interop.Outlook.Application oApp = new Microsoft.Office.Interop.Outlook.Application();

                // Get the MAPI namespace.
                Microsoft.Office.Interop.Outlook.NameSpace oNS = oApp.GetNamespace("mapi");

                // Log on by using the default profile or existing session (no dialog box).
                oNS.Logon(Missing.Value, Missing.Value, false, true);

                // Alternate logon method that uses a specific profile name.
                // TODO: If you use this logon method, specify the correct profile name
                // and comment the previous Logon line.
                //oNS.Logon("profilename",Missing.Value,false,true);

                Microsoft.Office.Interop.Outlook.MAPIFolder oInbox = null;
                //Get the Inbox folder.
                foreach (Microsoft.Office.Interop.Outlook.MAPIFolder folder in oNS.Folders)
                {
                    string name = folder.Name;
                    if (name == "andreas.schwentner@amium.at")
                    {
                        foreach (Microsoft.Office.Interop.Outlook.MAPIFolder boxes in folder.Folders)
                        {
                            string name2 = boxes.Name;
                            if ((name2 == "inbox") || (name2 == "Posteingang"))
                            {
                                oInbox = boxes;
                                break;
                            }
                        }
                    }
                }
                //         Microsoft.Office.Interop.Outlook.MAPIFolder oInbox = oNS.GetFolderFromID("invoice");// .GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderInbox);

                //Get the Items collection in the Inbox folder.
                Microsoft.Office.Interop.Outlook.Items oItems = oInbox.Items;


                System.Collections.Generic.SortedDictionary<DateTime, Microsoft.Office.Interop.Outlook.MailItem> msg = new System.Collections.Generic.SortedDictionary<DateTime, Microsoft.Office.Interop.Outlook.MailItem>();
                //7  (Microsoft.Office.Interop.Outlook.MailItem)oItems.Sort("Date");

                //   foreach (Microsoft.Office.Interop.Outlook.MailItem oMsg2 in (Microsoft.Office.Interop.Outlook.MailItem)oItems.Sort()
                {

                }
                //    oItems.Sort("Datum");

                oItems.GetFirst();
                //      Microsoft.Office.Interop.Outlook.MailItem oM;

                while (true)
                {
                    try
                    {
                        Microsoft.Office.Interop.Outlook.MailItem oMsg2 = (Microsoft.Office.Interop.Outlook.MailItem)oItems.GetNext();
                        if (!msg.ContainsKey(oMsg2.ReceivedTime))
                            msg.Add(oMsg2.ReceivedTime, oMsg2);
                    }
                    catch
                    {
                        break;
                    }
                }


                // Get the first message.
                // Because the Items folder may contain different item types,
                // use explicit typecasting with the assignment.
                Microsoft.Office.Interop.Outlook.MailItem oMsg = (Microsoft.Office.Interop.Outlook.MailItem)oItems.GetFirst(); ;

                //Output some common properties.
                Console.WriteLine(oMsg.Subject);
                Console.WriteLine(oMsg.SenderName);
                Console.WriteLine(oMsg.ReceivedTime);
                Console.WriteLine(oMsg.Body);

                //Check for attachments.
                int AttachCnt = oMsg.Attachments.Count;
                Console.WriteLine("Attachments: " + AttachCnt.ToString());

                //TO DO: If you use the Microsoft Outlook 10.0 Object Library, uncomment the following lines.
                /*if (AttachCnt > 0) 
                {
                for (int i = 1; i <= AttachCnt; i++) 
                 Console.WriteLine(i.ToString() + "-" + oMsg.Attachments.Item(i).DisplayName);
                }*/

                //TO DO: If you use the Microsoft Outlook 11.0 Object Library, uncomment the following lines.
                /*if (AttachCnt > 0) 
                {
                for (int i = 1; i <= AttachCnt; i++) 
                 Console.WriteLine(i.ToString() + "-" + oMsg.Attachments[i].DisplayName);
                }*/

                //Display the message.
                oMsg.Display(true); //modal

                //Log off.
                oNS.Logoff();

                //Explicitly release objects.
                oMsg = null;
                oItems = null;
                oInbox = null;
                oNS = null;
                oApp = null;
            }

            //Error handler.
            catch (System.Exception e)
            {
                Console.WriteLine("{0} Exception caught: ", e);
            }

            // Return value.
            return 0;
        }

    }
}
