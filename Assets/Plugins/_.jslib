mergeInto(LibraryManager.library, {

  InitVoices: function () {
    window.speechSynthesis.getVoices();
  },
  
  StopTTS: function () {
    window.speechSynthesis.cancel();
  },
  
  Speak: function (strPointer, volumePointer, ratePointer, pitchPointer) {
  console.log(volumePointer);
  console.log(ratePointer);
  console.log(pitchPointer);
    var str = Pointer_stringify(strPointer);
    var msg = new SpeechSynthesisUtterance(str);
    msg.lang = 'en-GB';
    msg.volume = 1; // 0 to 1
    msg.rate = 1; // 0.1 to 10
    msg.pitch = 0.5; //0 to 2
    // stop any TTS that may still be active
    //window.speechSynthesis.cancel();
	
	
	// select voice
	voices = window.speechSynthesis.getVoices().sort(function (a, b) {
      const aname = a.name.toUpperCase(), bname = b.name.toUpperCase();
      if ( aname < bname ) return -1;
      else if ( aname == bname ) return 0;
      else return +1;
  });
  
  // select creepy uk male voice if present
  for(i = 0; i < voices.length ; i++) {
    if(voices[i].name == "Google UK English Male"){
		msg.voice = voices[i];
		break;
	}
  }
  
    window.speechSynthesis.speak(msg);
  }
});