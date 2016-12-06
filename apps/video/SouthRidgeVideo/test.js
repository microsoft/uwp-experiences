  var NewText = React.createClass({
        render: function() {
         return (
             <h1>This is new test</h1>
                )
            }
    });
    
    class LikeButton extends React.Component {
  constructor() {
    super();
    this.state = {
      liked: false
    };
    this.handleClick = this.handleClick.bind(this);
  }
  handleClick() {
    this.setState({liked: !this.state.liked});
  }
  render() {
    const text = this.state.liked ? 'liked' : 'haven\'t liked';
    return (
      <div onClick={this.handleClick}>
        You {text} this. Click to toggle.
         <NewText />
      </div>
     
    )
  }
}

ReactDOM.render(
  <LikeButton />,
  document.getElementById('example')
);
    