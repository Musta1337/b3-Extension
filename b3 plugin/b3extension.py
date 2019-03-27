# Coding: UTF-8

# b3 extension plugin by leliel
# This plugin allow b3 to send rcon commands to game server console
# 16.03.2019 - initial release v0.9b
# - Plugin creation + Loading b3extension.dll InfinityScript, made by Musta1337 (discord: Musta#6382).
# - Added IS command: !afk.
# 24.03.2019 - 0.9.2b
# - Added commands executed by b3extension.dll InfinityScript, made by Musta1337 (discord: Musta#6382).
# - Added IS commands: !afk, !kill, !setafk, !suicide, !teleport.
# 25.03.2019 - v0.9.4b
# - IS Commands added: !ac130, !blockchat, !changeteam, !freeze.
# - Fixed IS commands code
# 26.03.2019 - v0.9.6b
# - IS Commands added: !gametype, !mode.
# - Added <reason> field to !kill command + improved IS commands code

__author__ = 'leliel'
__version__ = '2.0.0.2'

import b3
import b3.plugin


class B3ExtensionPlugin(b3.plugin.Plugin):

    _adminPlugin = None

    def onStartup(self):
        '''
        Initialize plugin.
        '''
        self._adminPlugin = self.console.getPlugin('admin')

        if not self._adminPlugin:
            self.error('Admin plugin required and not running properly')
            return False

        # Register our commands
        if 'commands' in self.config.sections():
            for cmd in self.config.options('commands'):
                level = self.config.get('commands', cmd)
                sp = cmd.split('-')
                alias = None
                if len(sp) == 2:
                    cmd, alias = sp

                func = self.getCmd(cmd)
                if func:
                    self._adminPlugin.registerCommand(self, cmd, level, func, alias)

        # Send debug message on console
        self.debug('b3 Extension Plugin v.%s by %s started' % (__version__, __author__))

    def getCmd(self, cmd):
        cmd = 'cmd_%s' % cmd
        if hasattr(self, cmd):
            func = getattr(self, cmd)
            return func
        return None

    def cmd_ac130(self, data, client, cmd=None):
        '''
        !ac130 <playername> - Provide AC-130 to a given player.
        '''
        m = self._adminPlugin.parseUserCmd(data)
        if not m:
            self.console.write('set sv_b3Execute "!ac130 %s"' % client.cid)
            client.message('^7You are now powered up with ^1AC-130!')
        elif m[0] == 'all':
            self.console.write('set sv_b3Execute "!ac130 *all*"')
            self.console.say('^1AC-130 ^7was provided to ^3everyone ^7by ^5%s^7[^5%s^7]' % (client.name, client.maxLevel))
        else:
            sclient = self._adminPlugin.findClientPrompt(m[0], client)
            self.console.write('set sv_b3Execute "!ac130 %s"' % sclient.cid)
            sclient.message('^1AC-130 ^7was provided by ^5%s^7[^5%s^7]' % (client.name, client.maxLevel))

    def cmd_afk(self, data, client, cmd=None):
        '''
        !afk - Move the command caller to spectating.
        '''
        self.console.write('set sv_b3Execute "!afk %s"' % client.cid)
        client.message('^3%s ^7moved to ^5SPECTATING.' % client.name)

    def cmd_blockchat(self, data, client, cmd=None):
        '''
        !blockchat <playername> - Block the given player's chat.
        '''
        m = self._adminPlugin.parseUserCmd(data)
        if not m:
            client.message('^1ERROR^0: ^7You must provide a player name.')
            return False

        sclient = self._adminPlugin.findClientPrompt(m[0], client)
        if sclient:
            if sclient.cid == client.cid:
                client.message('^1ERROR^0: ^7You can\'t disable your chat.')
            elif sclient.maxLevel >= client.maxLevel:
                client.message('^7You are too weak to ^1MUTE ^3%s^7.' % sclient.name)
                sclient.message('^1Warning^0: ^3%s ^7tried to ^1MUTE ^7you!' % client.name)
        else:
            self.console.write('set sv_b3Execute "!blockchat %s"' % sclient.cid)

    def cmd_changeteam(self, data, client, cmd=None):
        '''
        !changeteam <playername> - Change team of the given player.
        '''
        m = self._adminPlugin.parseUserCmd(data)
        if not m:
            self.console.write('set sv_b3Execute "!changeteam %s"' % client.cid)
            client.message('^7Your ^5TEAM SIDE ^7has been changed.')
        else:
            sclient = self._adminPlugin.findClientPrompt(m[0], client)
            self.console.write('set sv_b3Execute "!changeteam %s"' % sclient.cid)
            self.console.say('^3%s ^5TEAM ^7has been changed ^7by ^5%s^7[^5%s^7]' % (sclient.name, client.name, client.maxLevel))

    def cmd_freeze(self, data, client, cmd=None):
        '''
        !freeze <playername> - Freeze the given player's controls.
        '''
        m = self._adminPlugin.parseUserCmd(data)
        if not m:
            client.message('^1Error^0: ^7You must provide a player name.')
            return False

        sclient = self._adminPlugin.findClientPrompt(m[0], client)
        if sclient:
            if sclient.cid == client.cid:
                client.message('^1ERROR^0: ^7You can\'t freeze yourself.')
                return False
            elif sclient.maxLevel >= client.maxLevel:
                client.message('^7You are too weak to ^1FREEZE ^3%s^7.' % sclient.name)
                sclient.message('^1Warning^0: ^3%s ^7tried to ^1FREEZE ^7your controls!' % client.name)
        else:
            self.console.write('set sv_b3Execute "!freeze %s"' % sclient.cid)

    def cmd_gametype(self, data, client, cmd=None):
        '''
        !gametype <dsrname> <mapname> - Set given DSR and MAP in game.
        '''
        m = self._adminPlugin.parseUserCmd(data)
        if not m[0]:
            client.message('^1Error^0: ^7You must provide a valid DSR name.')
            return False
        if not m[1]:
            client.message('^1Error^0: ^7You must provide a valid MAP name.')
            return False
        else:
            self.console.write('set sv_b3Execute "!gametype %s %s"' % (m[0], m[1]))
            client.message('^3Mode has been changed to %s, Map has been changed to %s' % (m[0], m[1]))

    def cmd_kill(self, data, client, cmd=None):
        '''
        !kill <playername> - Kill a given player.
        '''
        m = self._adminPlugin.parseUserCmd(data)
        if not m:
            client.message('^1Error^0: ^7You must provide a player name.')
            return False
        else:
            sclient = self._adminPlugin.findClientPrompt(m[0], client)
            if sclient:
                if sclient.cid == client.cid:
                    client.message('^7Do you want kill youself? Type ^5!suicide ^7and enjoy!')
                elif sclient.maxLevel >= client.maxLevel:
                    client.message('^7You are too weak to ^1KILL ^3%s^7.' % sclient.name)
                    sclient.message('^1Warning^0: ^3%s ^7tried to ^1KILL ^7you!' % client.name)
                    return False
            else:
                self.console.write('set sv_b3Execute "!kill %s"' % sclient.cid)
                self.console.say('^3%s ^7has been ^1KILLED ^7by ^5%s^7[^5%s^7]' % (sclient.name, client.name, client.maxLevel))
                sclient.message('^1You ^7have been ^1KILLED ^7by ^5%s^7[^5%s^7]' % (client.name, client.maxLevel))

    def cmd_mode(self, data, client, cmd=None):
        '''
        !mode <dsrname> - Set given DSR in game.
        '''
        m = self._adminPlugin.parseUserCmd(data)
        if not m[0]:
            client.message('^1Error^0: ^7You must provide a valid DSR name.')
            return False
        else:
            self.console.write('set sv_b3Execute "!mode %s"' % m[0])
            client.message('^3Mode has been set to %s' % m[0])

    def cmd_setafk(self, data, client, cmd=None):
        ''' 
        !setafk <playername> - Move the given player as spectator.
        '''
        m = self._adminPlugin.parseUserCmd(data)
        if not m:
            client.message('^1Error^0: ^7You must provide a player name.')
            return False
        else:
            sclient = self._adminPlugin.findClientPrompt(m[0], client)
            if sclient.cid == client.cid:
                client.message('^7Want be a spectator? Use ^5!afk ^7command.')
                return False
            else:
                self.console.write('set sv_b3Execute "!setafk %s"' % sclient.cid)
                client.message('^7Player ^3%s ^7has been moved to ^5SPECTATING.' % sclient.name)

    def cmd_suicide(self, data, client, cmd=None):
        '''
        !suicide <playername> - Suicide command.
        '''
        self.console.write('set sv_b3Execute "!suicide %s"' % client.cid)
        client.message('^1Congratulations, you killed yourself!')

    def cmd_teleport(self, data, client, cmd=None):
        '''
        !teleport <playername1> <playername2> - Teleport player1 to player2.
        !teleport <playername> - Teleport command caller to a given player.
        '''
        m = self._adminPlugin.parseUserCmd(data)
        if not m:
            client.message('^1Error^0: ^7You must provide player name to teleport.')
            return False

        if not m[1]:
            reciever = client
        else:
            reciever = self._adminPlugin.findClientPrompt(m[1], client)

        teleporter = self._adminPlugin.findClientPrompt(m[0], client)
        if teleporter.cid == reciever.cid:
            client.message("^1Error^0: ^1both arguments can't be same.")
            return False
        if teleporter and reciever:
            self.console.write('set sv_b3Execute "!teleport %s %s"' % (teleporter.cid, reciever.cid))
            client.message("^3%s ^7has been ^2teleported ^7to ^5%s" % (teleporter.name, reciever.name))
