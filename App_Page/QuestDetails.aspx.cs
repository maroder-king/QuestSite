﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Model;
using Interface;
using Database;

public partial class App_Page_QuestDetails : System.Web.UI.Page
{
    private static PlayerModel PlayerModel { get; set; }
    private static QuestModel QuestModel { get; set; }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (PreviousPage != null)
        {
            PopulateQuestInfo(GetPlyaerInfo());
        }
        else
        {
            //Click handler for subscribe button per se
            //WTF?????? IT MUST NOT BE SO!!!!
            new DatabaseRequest<Object>()
            {
                RequestType = RequestType.SubscribePlayerForQuest,
                PlayerId = PlayerModel.Id,
                QuestId = QuestModel.Id
            }.Execute();
        }
    }

    private void PopulateQuestInfo(bool isUserLoggedIn)
    {
        if (PreviousPage is ICrossPageSender<QuestModel>)
        {
            ICrossPageSender<QuestModel> sourcePage = PreviousPage as ICrossPageSender<QuestModel>;
            QuestModel = sourcePage.GetModel();

            HtmlGenericControl questDetails = FindControl("QuestDetails") as HtmlGenericControl;
            AddQuestDescription(questDetails);
            PopulateSubscribersList(questDetails);

            if (isUserLoggedIn)
            {
                Button subscribeButton = new Button() { Text = "Принять участие", PostBackUrl = "~/App_Page/Profile.aspx" };
                questDetails.Controls.Add(subscribeButton);
            }
        }
    }

    private bool GetPlyaerInfo()
    {
        if (PreviousPage is ICrossPageSender<PlayerModel>)
        {
            ICrossPageSender<PlayerModel> sourcePage = PreviousPage as ICrossPageSender<PlayerModel>;
            PlayerModel = sourcePage.GetModel();

            if (PlayerModel != null)
            {
                HtmlGenericControl playerProfileInfo = FindControl("header") as HtmlGenericControl;

                Label userNickNameDeclaration = new Label() { Text = "Вы вошли как: " };
                HyperLink linkToProfilePage = new HyperLink()
                {
                    Text = "Вы вошли как: " + PlayerModel.NickName,
                    NavigateUrl = "~/App_Page/Profile.aspx"
                };
                playerProfileInfo.Controls.Add(userNickNameDeclaration);
                playerProfileInfo.Controls.Add(linkToProfilePage);
                return true;
            }
        }
        return false;
    }

    private void AddQuestDescription(HtmlGenericControl mainControl)
    {
        DatabaseResponse<List<StageModel>> stagesResponse = new DatabaseRequest<List<StageModel>>()
        {
            RequestType = RequestType.GetStages,
            QuestId = QuestModel.Id
        }.Execute();

        Label description = new Label()
        {
            Text = String.Format("{0}<br>{1}<br>Дата начала: {2}<br>Дата окончания: {3}<br>Уровень сложности: {4}<br>Количество этапов: {5}<br>",
            QuestModel.Name, QuestModel.Description, new DateTime(QuestModel.StartDate), new DateTime(QuestModel.ExpirationDate),
            QuestModel.ComplexityLevel, stagesResponse.ResponseModel.Count)
        };
        mainControl.Controls.Add(description);
    }

    private void PopulateSubscribersList(HtmlGenericControl mainControl)
    {
        DatabaseResponse<List<PlayerModel>> subscribersResponse = new DatabaseRequest<List<PlayerModel>>()
        {
            RequestType = Database.RequestType.GetQuestSubscribers,
            QuestId = QuestModel.Id
        }.Execute();

        if (subscribersResponse.ResponseModel != null)
        {
            HtmlGenericControl subscribersDiv = new HtmlGenericControl("div");
            Label subscribersHeaderText = new Label() { Text = "Подписчики:" };
            subscribersDiv.Controls.Add(subscribersHeaderText);

            HtmlGenericControl subscribersList = new HtmlGenericControl("ul");
            foreach (PlayerModel player in subscribersResponse.ResponseModel)
            {
                HtmlGenericControl listItem = new HtmlGenericControl("li");
                listItem.Controls.Add(new Label() { Text = player.ToString() });
                subscribersList.Controls.Add(listItem);
            }
            subscribersDiv.Controls.Add(subscribersList);
            mainControl.Controls.Add(subscribersDiv);
        }
    }

}