import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import '../services/sync_service.dart';
import 'trilhas_screen.dart';
import 'denuncia_screen.dart';
import 'login_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  @override
  void initState() {
    super.initState();
    
    // Dispara a sincronização automática em segundo plano ao abrir a tela inicial
    SyncService().sincronizarDenuncias().then((totalSincronizado) {
      if (totalSincronizado > 0 && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('$totalSincronizado denúncia(s) pendente(s) foram sincronizada(s) com sucesso!'),
            backgroundColor: Colors.blue,
          ),
        );
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Valoriza'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () async {
              await AuthService().logout();
              if (context.mounted) {
                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(builder: (_) => const LoginScreen()),
                );
              }
            },
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Olá! Bem-vindo(a)',
              style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            const Text(
              'Escolha uma das opções abaixo:',
              style: TextStyle(color: Colors.grey),
            ),
            const SizedBox(height: 24),
            _MenuCard(
              icon: Icons.school,
              titulo: 'Trilhas de Treinamento',
              descricao: 'Capacitação em diversidade étnico-racial',
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const TrilhasScreen()),
              ),
            ),
            const SizedBox(height: 12),
            _MenuCard(
              icon: Icons.report_problem_outlined,
              titulo: 'Canal de Denúncias',
              descricao: 'Registre de forma segura e confidencial',
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const DenunciaScreen()),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _MenuCard extends StatelessWidget {
  final IconData icon;
  final String titulo;
  final String descricao;
  final VoidCallback onTap;

  const _MenuCard({
    required this.icon,
    required this.titulo,
    required this.descricao,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 2,
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: Theme.of(context).colorScheme.primaryContainer,
          child: Icon(icon),
        ),
        title: Text(titulo, style: const TextStyle(fontWeight: FontWeight.w600)),
        subtitle: Text(descricao),
        trailing: const Icon(Icons.chevron_right),
        onTap: onTap,
      ),
    );
  }
}
