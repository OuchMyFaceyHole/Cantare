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
$(document).ready(async function () {
    const modal = new bootstrap.Modal(document.getElementById("modal"));
    audioData = await (await fetch('/GetSongAudioData')).arrayBuffer();

    gainSetter = audioContext.createGain();
    gainSetter.gain.value = 0.25;
    var progressBar = document.getElementById("PlayProgress");
    var songInputGroup = document.getElementById("SongInputGroup");
    songInput = document.getElementById("SongInput");
    songInput.innerHTML = "";
    songInput.value = "";
    guessContainer = document.getElementById("GuessContainer");
    calendarPageMax = parseInt(document.getElementById("pageMax").innerHTML);
    songDate = document.getElementById("SongDate").innerHTML;
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
                        songDate: document.getElementById("SongDate").innerHTML,
                        songGuess: songInput.dataset.trueValue,
                        guessCount: guessCount
                    },
                    success: async function (data) {
                        let resultData = JSON.parse(data);
                        guessResult[guessCount - 1] = resultData.GuessResult;
                        if (resultData.GuessResult != 0) {
                            var currentGuessRow = guessContainer.children[guessCount - 1];
                            currentGuessRow.style.backgroundColor = enumValToColor[resultData.GuessResult];
                            currentGuessRow.firstElementChild.firstElementChild.innerHTML = songInput.value;
                            currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
                            currentGuessRow.firstElementChild.firstElementChild.hidden = false;
                            songInput.value = "";
                            progressBar.style.width = (100 / 6) * guessCount + "%";
                            if (guessCount < 6) {
                                guessCount++;
                                guessContainer.children[guessCount - 1].children[0].children[0].hidden = true;
                                guessContainer.children[guessCount - 1].children[0].appendChild(songInputGroup);
                                guessContainer.children[guessCount - 1].children[0].style.backgroundColor = "";
                            }
                            else {
                                songInputGroup.hidden = true;
                                var currentGuessRow = guessContainer.children[6];
                                currentGuessRow.firstElementChild.firstElementChild.innerHTML =
                                    resultData.GuessData.Artist + " " + resultData.GuessData.AlbumTitle + " " + resultData.GuessData.SongTitle;
                                currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
                                currentGuessRow.firstElementChild.firstElementChild.hidden = false;
                                currentGuessRow.hidden = false;
                            }
                        }
                        else {
                            var currentGuessRow = guessContainer.children[guessCount - 1];
                            currentGuessRow.style.backgroundColor = enumValToColor[resultData.GuessResult];
                            currentGuessRow.firstElementChild.firstElementChild.innerHTML = songInput.value;
                            currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
                            currentGuessRow.firstElementChild.firstElementChild.hidden = false;
                            currentGuessRow.firstElementChild.hidden = false;
                            songInputGroup.hidden = true;
                            guessCount = 6;
                            progressBar.style.width = "100%";
                        }
                        if (guessCount == 6) {
                            document.getElementById("ShareButton").disabled = false;
                            document.getElementById("SkipButton").disabled = true;
                        }
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
            guessResult[guessCount - 1] = 5;
            var currentGuessRow = guessContainer.children[guessCount - 1];
            currentGuessRow.style.backgroundColor = enumValToColor[5];
            currentGuessRow.firstElementChild.firstElementChild.innerHTML = "Skipped";
            currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
            currentGuessRow.firstElementChild.firstElementChild.hidden = false;
            progressBar.style.width = (100 / 6) * guessCount + "%";
            if (guessCount < 6) {
                guessCount++;
                guessContainer.children[guessCount - 1].children[0].children[0].hidden = true;
                guessContainer.children[guessCount - 1].children[0].appendChild(songInputGroup);
                guessContainer.children[guessCount - 1].children[0].style.backgroundColor = "";
                if (guessCount == 6) {
                    document.getElementById("SkipButton").disabled = true;
                }
            }
            else {
                songInputGroup.hidden = true;
            }
        }
        else if (event.target.id === "ShareButton") {
            let shareString = "Cantare " + songDate + "\n🔊";
            for (let i = 0; i < guessCount; i++) {
                if (guessResult[i] != undefined) {
                    shareString += enumValToEmoji[guessResult[i]];
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
                    progressBar = document.getElementById("PlayProgress");
                    songInputGroup = document.getElementById("SongInputGroup");
                    songInput = document.getElementById("SongInput");
                    songInput.innerHTML = "";
                    songInput.value = "";
                    guessContainer = document.getElementById("GuessContainer");
                    guessCount = 1;
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
    if (false) {

    }
    else {
        let todaysGuesses = document.cookie.split(';').filter(val => val.includes(songDate));
        if (todaysGuesses.length > 0) {
            for (let i = 0; i < todaysGuesses.length; i++) {
                let currentGuessRow = guessContainer.children[i];
                let guess = todaysGuesses.find(val => val.includes(songDate + "*" + (i + 1))).split('=')[1];
                if (guess === 'Skipped') {
                    currentGuessRow.style.backgroundColor = enumValToColor[5];
                    currentGuessRow.firstElementChild.firstElementChild.innerHTML = guess;
                }
                else {
                    guess = JSON.parse(guess);
                    let guessData = guess.GuessData;
                    currentGuessRow.style.backgroundColor = enumValToColor[guess.GuessResult];
                    currentGuessRow.firstElementChild.firstElementChild.innerHTML =
                        guessData.Artist + '-' + guessData.AlbumTitle + '-' + guessData.SongTitle;
                }
                currentGuessRow.firstElementChild.firstElementChild.style.margin = "0.5rem";
                currentGuessRow.firstElementChild.firstElementChild.hidden = false;
            }
        }
    }
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