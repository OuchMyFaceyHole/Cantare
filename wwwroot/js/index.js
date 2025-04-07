let guessCount = 1;
let guessResult = new Array(6);
let enumValToColor = {
    0: "green",
    1: "orange",
    2: "yellow",
    3: "magenta",
    4: "red",
    5: "dimgrey"
}
let enumValToEmoji = {
    0: "🟩",
    1: "🟧",
    2: "🟨",
    3: "🟪",
    4: "🟥",
    5: "⬛"
}
let audioInterval = null;
let audioData = null;
let audioContext = new AudioContext();
let gainSetter = null;
let isPlaying = false;
let currentCalendarPage = 1;
let calendarPageMax = 0;
let songDate = "";
let guessContainer = null;
let songInput = null;
let progressBar = null;
let songInputGroup = null;
$(document).ready(async function () {
    InitRequiredElements();
    const modal = new bootstrap.Modal(document.getElementById("modal"));
    audioData = await (await fetch('/GetSongAudioData?' + new URLSearchParams({ date: songDate }))).arrayBuffer();
    gainSetter = audioContext.createGain();
    gainSetter.gain.value = 0.25;
    calendarPageMax = parseInt(document.getElementById("pageMax").innerHTML);
    LoadGuessData();
    document.addEventListener('click', async function (event) {
        if (event.target.id === "PlayButton" && !isPlaying) {
            await PlaySong();
        }
        else if (event.target.id === "SubmitButton") {
            if (workingSet.includes(songInput.dataset.trueValue)) {
                $.ajax({
                    url: '/SongGuess',
                    type: 'POST',
                    data: {
                        songDate: songDate,
                        songGuess: songInput.dataset.trueValue,
                        guessCount: guessCount
                    },
                    success: async function (data) {
                        let resultData = JSON.parse(data);
                        FillGuessValues(resultData.GuessResult, resultData.GuessData, resultData.CorrectSong)
                        await PlaySong();
                    }
                });
                workingSet = [];
            }
            else {
                ElementError(songInput);
            }
        }
        else if (event.target.id === "SkipButton") {
            $.ajax({
                url: '/SongSkip',
                type: "POST",
                data: {
                    songDate: songDate
                },
                success: async function (data) {
                    FillGuessValues(5, null, null);
                    await PlaySong();
                }
            })
        }
        else if (event.target.id === "ShareButton") {
            let shareString = "Cantare " + songDate + "\n🔊";
            for (let i = 1; i < guessCount; i++) {
                if (guessResult[i -1] != undefined) {
                    shareString += enumValToEmoji[guessResult[i - 1]];
                }
                else {
                    shareString += "⬜";
                }
            }
            navigator.clipboard.writeText(shareString);
        }
        else if (event.target.id === "CalendarSelect") {
            let date = event.target.dataset.date;
            $.ajax({
                url: '/GetPlayingArea',
                type: 'GET',
                data: { date: date },
                success: async function (data) {
                    modal.toggle();
                    audioData = await (await fetch('/GetSongAudioData?' + new URLSearchParams({ date: date }))).arrayBuffer();
                    document.getElementById("PlayingArea").remove();
                    let main = document.getElementsByClassName("pb-3")[0];
                    main.innerHTML = data + main.innerHTML;
                    InitRequiredElements();
                    guessCount = 1;
                    LoadGuessData();
                    workingSet = [];
                    document.getElementById("ShareButton").disabled = true;
                    document.getElementById("SkipButton").disabled = false;
                }
            })
        }
        else if (event.target.id === "CloseModal" || event.target.id === "OpenCalendar") {
            if (event.target.dataset.type == 'calendar') {
                GetCalendarData();
            }
            modal.toggle();
        }
        else if (event.target.classList.contains("page-link")) {
            var innerData = event.target.innerHTML;
            if (innerData === "Previous") {
                if (currentCalendarPage > 1) {
                    document.getElementById("PageSelect-" + currentCalendarPage).classList.remove("active");
                    currentCalendarPage--;
                    document.getElementById("PageSelect-" + currentCalendarPage).classList.add("active");
                    GetCalendarData();
                }
            }
            else if (innerData === "Next") {
                if (currentCalendarPage < calendarPageMax) {
                    document.getElementById("PageSelect-" + currentCalendarPage).classList.remove("active");
                    currentCalendarPage++;
                    document.getElementById("PageSelect-" + currentCalendarPage).classList.add("active");
                    GetCalendarData();
                }
            }
            else {
                if (parseInt(innerData) !== currentCalendarPage) {
                    document.getElementById("PageSelect-" + currentCalendarPage).classList.remove("active");
                    currentCalendarPage = parseInt(event.target.innerHTML);
                    document.getElementById("PageSelect-" + currentCalendarPage).classList.add("active");
                    GetCalendarData();
                }
            }
        }
    });
});

function InitRequiredElements() {
    songDate = document.getElementById("SongDate").innerHTML;
    progressBar = document.getElementById("PlayProgress");
    songInputGroup = document.getElementById("SongInputGroup");
    songInput = document.getElementById("SongInput");
    songInput.innerHTML = "";
    songInput.value = "";
    guessContainer = document.getElementById("GuessContainer");
}

function ElementError(element) {
    element.classList.add("ahashakeheartache");
    element.style.outlineColor = "red";
    element.style.outlineStyle = "solid";
    setTimeout(() => {
        element.classList.remove("ahashakeheartache");
        element.style.outlineColor = "";
        element.style.outlineStyle = "";
    }, 500);
}

function GetCalendarData() {
    $.ajax({
        url: '/GetCalendarData',
        type: 'GET',
        data: { page: (currentCalendarPage - 1) },
        success: function (data) {
            document.getElementById("Calendar").innerHTML = data;
        }
    });
}

function LoadGuessData() {
    $.ajax({
        url: '/GetGuessData',
        type: 'GET',
        data: { songDate },
        success: function (data) {
            var guessData = JSON.parse(data);
            guessData.Guesses.forEach((guess) => {
                FillGuessValues(guess.GuessStatus, guess.Song, guessData.CorrectSong);
            });
        }
    });
}

function FillGuessValues(guessInputResult, guessInputData, correctSong) {
    guessResult[guessCount - 1] = guessInputResult;
    var currentGuessRow = guessContainer.children[guessCount - 1];
    currentGuessRow.style.backgroundColor = enumValToColor[guessInputResult];
    currentGuessRow.firstElementChild.firstElementChild.innerHTML = guessInputResult == 5 ? "Skipped" : `${guessInputData.Artist} ${guessInputData.AlbumTitle} ${guessInputData.SongTitle}`;
    currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
    currentGuessRow.firstElementChild.firstElementChild.hidden = false;

    if (guessInputResult != 0) {
        songInput.value = "";
        progressBar.style.width = (100 / 6) * guessCount + "%";
        if (guessCount < 6) {
            guessContainer.children[guessCount].children[0].children[0].hidden = true;
            guessContainer.children[guessCount].children[0].appendChild(songInputGroup);
            guessContainer.children[guessCount].children[0].style.backgroundColor = "";
        }
        else {
            songInputGroup.hidden = true;
            var currentGuessRow = guessContainer.children[6];
            currentGuessRow.firstElementChild.firstElementChild.innerHTML =
                correctSong.Artist + " " + correctSong.AlbumTitle + " " + correctSong.SongTitle;
            currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
            currentGuessRow.firstElementChild.firstElementChild.hidden = false;
            currentGuessRow.hidden = false;
        }
    }
    else {
        currentGuessRow.firstElementChild.hidden = false;
        songInputGroup.hidden = true;
        guessCount = 6;
        progressBar.style.width = "100%";
    }
    if (guessCount == 6) {
        document.getElementById("ShareButton").disabled = false;
        document.getElementById("SkipButton").disabled = true;
    }

    guessCount++;
}

async function PlaySong() {
    let audioBuffer = await audioContext.decodeAudioData(audioData.slice(0));
    audioSource = audioContext.createBufferSource();
    audioSource.addEventListener("ended", () => {
        isPlaying = false;
    });
    audioSource.buffer = audioBuffer;
    audioSource.connect(gainSetter).connect(audioContext.destination);
    audioSource.start(0, 0, guessCount);
    isPlaying = true;
}