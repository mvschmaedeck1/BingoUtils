﻿using BingoUtils.UI.Shared.Views.UserControls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace BingoUtils.UI.BingoPlayer.ViewModel.Pages
{
    public class CreateGameViewModel : ViewModelBase
    {
        public ObservableCollection<QuestionHolder> AddedQuestions { get; private set; }

        public RelayCommand<string> LastGotFocusCommand { get; private set; }
        public RelayCommand<string> ValidateTextCommand { get; private set; }
        public RelayCommand SaveGameCommand { get; private set; }

        public Brush SubjectBorderBrush { get; set; } = Brushes.LightGray;
        public Brush TopicBorderBrush { get; set; } = Brushes.LightGray;

        public string Subject { get; set; }
        public string Topic { get; set; }

        public bool SaveOnDefaults { get; set; }

        public CreateGameViewModel()
        {
            InitializeCommands();

            AddedQuestions = new ObservableCollection<QuestionHolder>();

            AddedQuestions.Add(new QuestionHolder());
        }

        private void InitializeCommands()
        {
            LastGotFocusCommand = new RelayCommand<string>((x) =>
            {
                var last = AddedQuestions.LastOrDefault();

                if (last == null || (!string.IsNullOrEmpty(last?.Title) || !string.IsNullOrEmpty(last?.Answer)))
                {
                    var holder = new QuestionHolder();
                    AddedQuestions.Add(holder);

                    holder.Loaded += (s, e) => LastGotFocusCommand.Execute(x);
                }

                if (x == "Title")
                {
                    AddedQuestions.LastOrDefault()?.FocusTextBoxTitle();
                }
                else
                {
                    AddedQuestions.LastOrDefault()?.FocusTextBoxAnswer();
                }
            });

            ValidateTextCommand = new RelayCommand<string>((x) =>
            {
                if(x == "Subject")
                {
                    SubjectBorderBrush = (string.IsNullOrWhiteSpace(Subject) ? Brushes.Red : Brushes.LightGray);
                }
                else if(x == "Topic")
                {
                    TopicBorderBrush = (string.IsNullOrWhiteSpace(Topic) ? Brushes.Red : Brushes.LightGray);
                }
            });

            SaveGameCommand = new RelayCommand(() =>
            {
                try
                {
                    SaveGame();
                }
                catch(Exception e)
                {
                    MessageBox.Show(string.Format("Falha ao criar arquivo do jogo.\n\nErro: {0}", e.Message), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void SaveGame()
        {
            if(string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Topic))
            {
                MessageBox.Show("Preencha todos os campos obrigatórios antes de continuar", "ERRO:", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Bingo", "Jogos", Subject);
            string tempPath = Path.Combine(Path.GetTempPath(), "BingoTemp", "CreatedGame");
            string imgsPath = Path.Combine(tempPath, "img");

            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }

            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(imgsPath);

            string file = Path.Combine(tempPath, "Game.csv");
            string fileContent;

            StringBuilder builder = new StringBuilder();

            builder.AppendLine(string.Format("{0};{1}", Subject, Topic));

            foreach(QuestionHolder holder in AddedQuestions)
            {
                string TitleImageName = string.Empty; 
                string AnswerImageName = string.Empty;

                if(!string.IsNullOrEmpty(holder.TitleImagePath))
                {
                    if (File.Exists(holder.TitleImagePath))
                    {
                        TitleImageName = Path.GetFileName(holder.TitleImagePath);
                        File.Copy(holder.TitleImagePath, Path.Combine(imgsPath, TitleImageName));
                    }
                    else
                    {
                        MessageBox.Show(string.Format("A seguinte imagem não foi encontrada:\n\n{0}\n\nA imagem não foi adicionada ao jogo.", TitleImageName), "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                if (!string.IsNullOrEmpty(holder.AnswerImagePath))
                {
                    if (File.Exists(holder.AnswerImagePath))
                    {
                        AnswerImageName = Path.GetFileName(holder.AnswerImagePath);
                        File.Copy(holder.AnswerImagePath, Path.Combine(imgsPath, AnswerImageName));
                    }
                    else
                    {
                        MessageBox.Show(string.Format("A seguinte imagem não foi encontrada:\n\n{0}\n\nA imagem não foi adicionada ao jogo.", TitleImageName), "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                builder.AppendLine(string.Format("{0};{1};{2};{3};{4}", holder.Title, holder.Answer, TitleImageName, holder.IsImageImportant, AnswerImageName));
            }

            fileContent = builder.ToString();

            using (StreamWriter writer = new StreamWriter(file, false, Encoding.GetEncoding("WINDOWS-1252")))
            {
                writer.Write(fileContent);
            }

            string zipPath = Path.Combine(tempPath, string.Format("{0}.zip", Topic));

            if(File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            using (ZipArchive newFile = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                newFile.CreateEntryFromFile(file, "Game.csv");

                foreach (string imgName in Directory.GetFiles(imgsPath))
                {
                    newFile.CreateEntryFromFile(imgName, Path.Combine("img", Path.GetFileName(imgName)));
                }
            }

            if (SaveOnDefaults)
            {
                string formattedZipName = Path.Combine(path, string.Format("{0}.zip", Topic));

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (File.Exists(formattedZipName))
                {
                    var result = MessageBox.Show("Já existe um jogo com este nome nos seus jogos padrão, deseja sobrescrevê-lo?", "ATENÇÃO", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                File.Copy(zipPath, formattedZipName, true);
            }

            SaveFileDialog SaveFileDialog = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = "zip",
                FileName = Topic,
            };

            if (SaveFileDialog.ShowDialog() == true)
            {
                File.Copy(zipPath, SaveFileDialog.FileName, true);
            }
        }
    }
}
