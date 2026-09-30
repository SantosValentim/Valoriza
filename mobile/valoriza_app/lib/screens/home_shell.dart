import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import 'dashboard_screen.dart';
import 'treinamentos_screen.dart';
import 'mentorias_screen.dart';
import 'denuncia_screen.dart';
import 'indicadores_screen.dart';
import 'comunicacao_screen.dart';
import 'perfil_screen.dart';
import 'login_screen.dart';

class HomeShell extends StatefulWidget {
  const HomeShell({super.key});

  @override
  State<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends State<HomeShell> {
  int _index = 0;
  bool _loading = true;
  bool _gestorAdmin = false;
  bool _gestorDEI = false;

  @override
  void initState() {
    super.initState();
    _roles();
  }

  Future<void> _roles() async {
    final a = AuthService();
    _gestorAdmin = await a.isGestorOuAdmin();
    _gestorDEI = await a.isGestorDEI();
    setState(() => _loading = false);
  }

  List<_Item> get _items {
    final list = <_Item>[];
    if (_gestorAdmin) {
      list.add(_Item('Início', Icons.dashboard, const DashboardScreen()));
    }
    list.add(_Item('Trilhas', Icons.school, const TreinamentosScreen()));
    list.add(_Item('Mentorias', Icons.people, const MentoriasScreen()));
    list.add(_Item('Denúncias', Icons.report_outlined, const DenunciasScreen()));
    if (_gestorAdmin) {
      list.add(_Item('Indicadores', Icons.bar_chart, const IndicadoresScreen()));
    }
    if (_gestorDEI) {
      list.add(_Item('Inclusiva', Icons.forum, const ComunicacaoScreen()));
    }
    list.add(_Item('Perfil', Icons.person, const PerfilScreen()));
    return list;
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    final items = _items;
    if (_index >= items.length) _index = 0;

    return Scaffold(
      appBar: AppBar(
        title: Text(items[_index].label),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () async {
              await AuthService().logout();
              if (!context.mounted) return;
              Navigator.of(context).pushReplacement(
                MaterialPageRoute(builder: (_) => const LoginScreen()),
              );
            },
          ),
        ],
      ),
      body: items[_index].page,
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (i) => setState(() => _index = i),
        destinations: items
            .map((e) => NavigationDestination(icon: Icon(e.icon), label: e.label))
            .toList(),
      ),
    );
  }
}

class _Item {
  final String label;
  final IconData icon;
  final Widget page;
  _Item(this.label, this.icon, this.page);
}