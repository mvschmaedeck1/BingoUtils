﻿using BingoUtils.Domain.Entities;
using BingoUtils.Domain.Enums;
using BingoUtils.Helpers;
using BingoUtils.UI.BingoPlayer.Views.Windows;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace BingoUtils.UI.BingoPlayer.ViewModel.Pages
{
    public class CardGeneratorViewModel : BaseViewModel
    {
        private const double MAX_SIMILARITY = 75;

        private int _SelectedIndexSubject;
        private IEnumerable<string> _AvaliableSubjects;
        private List<Question> _GameQuestions;
        private BackgroundWorker _WorkerToDistribute;
        
        public int SelectedIndexSubject
        {
            get
            {
                return _SelectedIndexSubject;
            }
            set
            {
                Set(ref _SelectedIndexSubject, value);
                AvaliableTopics = GameHelper.GetAvaliableTopicsForSubject(AvaliableSubjects.ElementAt(SelectedIndexSubject));
                
            }
        }
        public int SelectedIndexTopic { get; set; }
        public double? AmountOfCards { get; set; }
        public double? AmountOfQuestionsPerCard { get; set; }
        public string ErrorText { get; set; }

        public IEnumerable<string> AvaliableSubjects
        {
            get
            {
                return _AvaliableSubjects;
            }
            set
            {
                Set(ref _AvaliableSubjects, value);
                AvaliableTopics = GameHelper.GetAvaliableTopicsForSubject(AvaliableSubjects.ElementAt(SelectedIndexSubject));
            }
        }
        public IEnumerable<string> AvaliableTopics { get; set; }

        public CardGeneratorStatus CurrentDistributorStatus { get; set; }

        public ICommand DistributeQuestionsCommand { get; private set; }
        public ICommand GenerateCardsCommand { get; private set; }
        public ICommand GeneratedCardsCommand { get; set; }

        public CardGeneratorViewModel()
        {
            InitializeCommands();
            AvaliableSubjects = GameHelper.GetAvaliableSubjects();
        }

        private void InitializeCommands()
        {
            DistributeQuestionsCommand = new RelayCommand(() =>
            {
                _GameQuestions = GameHelper.LoadGame(AvaliableSubjects.ElementAt(SelectedIndexSubject), AvaliableTopics.ElementAt(SelectedIndexTopic), "Card");

                switch (ValidateInputs())
                {
                    case 1:
                        CurrentDistributorStatus = CardGeneratorStatus.Waiting;
                        ErrorText = "A quantidade de questões não pode ser zero.";
                        return;
                    case 2:
                        CurrentDistributorStatus = CardGeneratorStatus.Waiting;
                        ErrorText = "A quantidade de cartelas não pode ser zero.";
                        return;
                    case 3:
                        CurrentDistributorStatus = CardGeneratorStatus.Waiting;
                        ErrorText = "A quantidade de questões por cartela não pode ser zero.";
                        return;
                    case 4:
                        CurrentDistributorStatus = CardGeneratorStatus.Waiting;
                        ErrorText = "A quantidade de questões por cartela não pode ser maior que a quantidade de questões.";
                        return;
                }
                
                DistributeQuestions();
            });
        }

        private void DistributeQuestions()
        {
            bool succeeded = false;
            Card[] cards = null;

            _WorkerToDistribute = new BackgroundWorker();

            _WorkerToDistribute.DoWork += (s, e) =>
            {
                succeeded = DistributeQuestions(out cards);
            };

            _WorkerToDistribute.RunWorkerCompleted += (s, e) =>
            {
                CurrentDistributorStatus = succeeded ? CardGeneratorStatus.Success : CardGeneratorStatus.Error;

                SaveFileDialog dialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    DefaultExt = ".pdf",
                    Filter = "PDF|*.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    var window = new PdfGeneratorWindow(_GameQuestions, cards, dialog.FileName);

                    window.Closed += (sender, eargs) =>
                    {
                        GeneratedCardsCommand?.Execute(null);
                    };

                    window.Show();
                }
                
            };

            CurrentDistributorStatus = CardGeneratorStatus.Working;

            _WorkerToDistribute.RunWorkerAsync();
        }

        /* This method needs improvements
         * TODO
         * - Implement a logic to distribute the questions using all of them and using them in a similar amount of times
        */
        private bool DistributeQuestions(out Card[] cards)
        {
            Random r = new Random();
            cards = new Card[(int) AmountOfCards];
            int i = 0;

            do
            {
                cards[i] = new Card(i, (int) AmountOfQuestionsPerCard);

                while(!cards[i].IsFull)
                {
                    int val = r.Next(Convert.ToInt32(_GameQuestions.Count)) + 1;
                    cards[i].AddQuestion(val);
                }

                double maxSimilarity = 0;

                for(int j = i - 1; j >= 0; j--)
                {
                    double comparedCardSimilarity = cards[i].GetSimilarity(cards[j]);

                    if(comparedCardSimilarity > maxSimilarity)
                    {
                        maxSimilarity = comparedCardSimilarity;
                    }
                }

                if(maxSimilarity <= MAX_SIMILARITY)
                {
                    i++;
                }

            } while(i < AmountOfCards);

            return true;
        }

        private int ValidateInputs()
        {
            if(_GameQuestions.Count <= 0)
            {
                return 1;
            }
            else if (AmountOfCards == null || AmountOfCards <= 0)
            {
                return 2;
            }
            else if(AmountOfQuestionsPerCard == null || AmountOfCards <= 0)
            {
                return 3;
            }
            else if (_GameQuestions.Count < AmountOfQuestionsPerCard)
            {
                return 4;
            }
            return 0;
        }
    }
}