# WindowPong

<h1>Overview</h1>
A game of pong using active windows written in C# using the Windows API. This was a side project I worked on for fun. The code uses a game loop to drive the game and uses a basic game object structure (GameObjects, Colliders, etc) to drive the paddles and ball. The game is not finished and several bugs are present. <b>Refer to known bug list below</b>

<b>I wrote this "game" in 2019 and have decided to publish it</b>

<h1>Disclaimer</h1>
This game uses several native Win32 API calls such as (SetWindowPos, GetWindowRect, etc) which may or may not be flagged by antivirus software. The release is also not digitally signed, therefore Windows might block the execution of the program with the SmartScreen. <i>It is safe to bypass this warning.</i>

<h2><b>PLEASE DO NOT USE THIS CODE FOR MALICIOUS PURPOSES. THIS WAS MADE FOR FUN ONLY. I DO NOT TAKE ANY RESPONSIBILITY FOR ANY DAMAGE DONE USING THIS PROGRAM OR ANY FORMS OF IT</b></h2>

<h1>How to run</h1>
The project can either be compiled from the source code or downloaded from the <b>Releases</b> section on this repository.

<h1>Known Bugs</h1>
<ul>
  <li>Collisions are not accurate at high speeds for some windows.</li>
  <li>Sometimes when launching and the initial hook fails, the score board counts up sporadically and the game doesn't work. A restart usually fixes this.</li>
  <li>The ball can get stuck in an infinite loop of bouncing up and down if the bounce angle is too steep. This is rare.</li>
  <li>The window responsible for the ball's size may not always change to the specified size due to limitations with the window itself.</li>
</ul>

<h1>Gameplay</h1>
<a href="https://youtu.be/oK2VVlvyIA4"><img src="http://img.youtube.com/vi/oK2VVlvyIA4/0.jpg" title="Window Pong Gameplay Demo"/></a>

<h1>Planned Features</h1>
<ul>
  <li>Sound Effects</li>
  <li>Multiplayer (Maybe)</li>
  <li>User interface</li>
  <li>In-depth comments</li>
</ul>

<h1>Credits</h1>
<b>Kongtext.ttf font is a completely free-to-use font. I have uploaded it to this reposity as this program relies on it!</b>
