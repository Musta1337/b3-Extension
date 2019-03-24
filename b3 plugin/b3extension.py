__version__ = '1.0'
__author__  = 'Musta'

import b3
import b3.plugin
import b3.events
import datetime

class B3ExtensionPlugin(b3.plugin.Plugin):
    _adminPlugin = None

    def startup(self):
      """\
      Initialize plugin settings
      """
      self._adminPlugin = self.console.getPlugin('admin')
      if not self._adminPlugin:
        self.error('Could not find admin plugin')
        return False

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

      self.debug('b3extension plugin has started.')



    def getCmd(self, cmd):
      cmd = 'cmd_%s' % cmd
      if hasattr(self, cmd):
        func = getattr(self, cmd)
        return func

      return None

    def cmd_afk(self, data, client, cmd=None):
      self.console.write('set sv_b3Execute "!afk %s"' % client.name)
      client.message('^3You are now spectating.')
      return True
    
    def cmd_setafk(self, data, client, cmd=None):
      m = self._adminPlugin.parseUserCmd(data)
      if not m[0]:
          client.message('^1Error^0: ^7You must provide player name.')
          return False
      else:
          sclient = self._adminPlugin.findClientPrompt(m[0], client)
          if sclient:
              self.console.write('set sv_b3Execute "!setafk %s"' % sclient.name)
              client.message('^3player %s has been set afk.' % sclient.name)
    
    def cmd_mode(self, data, client, cmd=None):
      m = self._adminPlugin.parseUserCmd(data)
      if not m[0]:
          client.message('^1Error^0: ^7You must provide a dsr name.')
          return False
      else:
          self.console.write('set sv_b3Execute "!mode %s"' % m[0])
          client.message('^3Mode has been set to %s' % m[0])

    def cmd_gametype(self, data, client, cmd=None):
      m = self._adminPlugin.parseUserCmd(data)
      if not m[0]:
          client.message('^1Error^0: ^7You must provide mode name.')
          return False
      if not m[1]:
          client.message('^1Error^0: ^7You must provide map name.')
          return False
      else:
          self.console.write('set sv_b3Execute "!gametype %s %s"' % (m[0], m[1]))
          client.message('^3Mode has been changed to %s, Map has been changed to %s' % (m[0], m[1]))

    def cmd_kill(self, data, client, cmd=None):
      m = self._adminPlugin.parseUserCmd(data)
      if not m[0]:
          client.message('^1Error^0: ^7You must provide player name.')
          return False
      else:
          sclient = self._adminPlugin.findClientPrompt(m[0], client)
          if sclient:
              self.console.write('set sv_b3Execute "!kill %s"' % sclient.name)
              client.message('^1%s has been killed.' % sclient.name)
              sclient.message('^1You have been killed by %s[%s]' % (client.name, client.maxLevel))
          return True

    def cmd_suicide(self, data, client, cmd=None):
      '''
      Suicide cmd
      '''
      self.console.write('set sv_b3Execute "!suicide %s"' % client.name)
      client.message('^1Congratulations, you killed yourself.')
      return True
    def cmd_teleport(self, data, client, cmd=None):
      '''
      teleport command
      '''
      m = self._adminPlugin.parseUserCmd(data)

      if not m[0]:
        client.message("^1Error^0: ^1You must provide a player to teleport.")
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
        self.console.write('set sv_b3Execute "!teleport %s %s"' % (teleporter.name, reciever.name))
        client.message("^1%s has been teleported to %s" % (teleporter.name, reciever.name)) 
      