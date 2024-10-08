using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.EGui.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Design.Statements
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExecutionPin))]
    public class TtGraphElement_ExecuteSequenceOutPin : TtGraphElement_ExecutionPin
    {
        class PinAddCmdData : IOperationCommandData
        {
            public int InsertIndex = -1;
            public TtExecuteSequenceOutPinDescription SrcOutPin;
            public TtExecuteSequenceOutPinDescription AddedPin;
        }

        public TtGraphElement_ExecuteSequenceOutPin(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            base.ConstructContextMenu(ref context, popupMenu);

            var parentMenu = popupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            parentMenu.AddMenuSeparator("Sequence");

            var executeSequence = ExecutionPinDescription.Parent as TtExecuteSequenceStatementDescription;
            var outPin = ExecutionPinDescription as TtExecuteSequenceOutPinDescription;
            var cmdData = new PinAddCmdData()
            {
                AddedPin = new TtExecuteSequenceOutPinDescription(),
                SrcOutPin = outPin,
                InsertIndex = executeSequence.GetExecuteOutPinIndex(outPin)
            };
            if (outPin.Deleteable)
            {
                parentMenu.AddMenuItem("Insert pre Pin", null, (TtMenuItem item, object sender) =>
                {
                    cmdHistory.CreateAndExtuteCommand("Insert pre Pin",
                        cmdData,
                        (data) =>
                        {
                            var cmdData = data as PinAddCmdData;
                            executeSequence.InsertExecuteOutPin(cmdData.InsertIndex, cmdData.AddedPin);
                        },
                        cmdData,
                        (data) =>
                        {
                            var cmdData = data as PinAddCmdData;
                            executeSequence.RemoveExecuteOutPin(cmdData.AddedPin);
                        });
                });
            }
            parentMenu.AddMenuItem("Add after pin", null, (TtMenuItem item, object sender) =>
            {
                cmdHistory.CreateAndExtuteCommand("Add after pin",
                    cmdData,
                    (data) =>
                    {
                        var cmdData = data as PinAddCmdData;
                        executeSequence.InsertExecuteOutPin(cmdData.InsertIndex + 1, cmdData.AddedPin);
                    },
                    cmdData,
                    (data) =>
                    {
                        var cmdData = data as PinAddCmdData;
                        executeSequence.RemoveExecuteOutPin(cmdData.AddedPin);
                    });
            });
            if(outPin.Deleteable)
            {
                parentMenu.AddMenuItem("Delete pin", null, (TtMenuItem item, object sender) =>
                {
                    cmdHistory.CreateAndExtuteCommand("Delete pin",
                        cmdData,
                        (data) =>
                        {
                            var cmdData = data as PinAddCmdData;
                            executeSequence.RemoveExecuteOutPin(cmdData.SrcOutPin);
                        },
                        cmdData,
                        (data) =>
                        {
                            var cmdData = data as PinAddCmdData;
                            executeSequence.InsertExecuteOutPin(cmdData.InsertIndex, cmdData.SrcOutPin);
                        });
                });
            }
        }
    }
}
