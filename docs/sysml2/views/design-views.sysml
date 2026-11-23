// Views rendered into docs/design/generated/ for the Design document.
// Per SysML2Tools syntax, 'expose' is only valid inside a named 'view' usage
// (not a 'view def' definition), and it - not 'render' - is what scopes the
// rendered diagram content. The ReviewMark facade re-exports every item.
package ReviewMarkDesignViews {
    private import StandardViewDefinitions::*;
    private import ReviewMark::*;

    view SoftwareStructureView : InterconnectionView {
        expose ReviewMark::ReviewMarkSystem;
    }

    view ReviewMarkView {
        expose ReviewMarkSystem;
    }

    view CliView {
        expose CliSubsystem;
    }

    view ConfigurationView {
        expose ConfigurationSubsystem;
    }

    view IndexingView {
        expose IndexingSubsystem;
    }

    view SelfTestView {
        expose SelfTestSubsystem;
    }
}
